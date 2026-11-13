using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Animal_Spa.Models;

namespace Animal_Spa.Services
{
    /// <summary>
    /// Offline demo repository. This is NOT a substitute for an authoritative server:
    /// on backend integration, move the transactional conflict check to the API/database.
    /// </summary>
    public sealed class BookingWorkflowStore
    {
        public sealed class WorkflowData
        {
            public List<ReceptionistBooking> Bookings { get; set; } = new();
            public List<SpaServiceSession> Sessions { get; set; } = new();
            public List<SpaRoom> Rooms { get; set; } = new();
            public List<SpaStaff> Staff { get; set; } = new();
        }

        private static readonly Lazy<BookingWorkflowStore> Shared = new(() => new BookingWorkflowStore());
        public static BookingWorkflowStore Current => Shared.Value;
        private readonly object _gate = new();
        private readonly string _path;
        private readonly JsonSerializerOptions _json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
        private WorkflowData _state;

        public BookingWorkflowStore(string? dataFile = null)
        {
            _path = dataFile ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AnimalSpa", "receptionist-workflow.json");
            if (File.Exists(_path))
            {
                try
                {
                    _state = JsonSerializer.Deserialize<WorkflowData>(File.ReadAllText(_path), _json)
                        ?? throw new InvalidDataException("File dữ liệu trống.");
                    if (_state.Bookings == null || _state.Sessions == null || _state.Rooms == null || _state.Staff == null)
                        throw new InvalidDataException("File dữ liệu thiếu danh sách bắt buộc.");
                }
                catch (Exception e) when (e is JsonException || e is InvalidDataException)
                {
                    throw new BookingWorkflowException($"Không đọc được dữ liệu booking tại {_path}. Hãy sao lưu/kiểm tra file JSON. Chi tiết: {e.Message}");
                }
            }
            else
            {
                _state = MakeDemoData();
                Persist();
            }
        }

        private T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, _json), _json)!;

        public List<ReceptionistBooking> GetBookings()
        {
            lock (_gate) return Clone(_state.Bookings.OrderByDescending(b => b.BookingTime).ToList());
        }

        public ReceptionistBooking GetBooking(long bookingId)
        {
            lock (_gate) return Clone(RequireBooking(bookingId));
        }

        public SpaServiceSession? GetSession(long bookingId)
        {
            lock (_gate)
            {
                SpaServiceSession? s = _state.Sessions.FirstOrDefault(x => x.BookingId == bookingId && x.Status != "CANCELLED");
                return s == null ? null : Clone(s);
            }
        }

        public string GetSessionSummary(long bookingId)
        {
            lock (_gate)
            {
                SpaServiceSession? session = _state.Sessions.FirstOrDefault(x => x.BookingId == bookingId && x.Status != "CANCELLED");
                if (session == null) return "Chưa phân công service session.";
                string room = _state.Rooms.FirstOrDefault(r => r.Id == session.RoomId)?.Name ?? $"#{session.RoomId}";
                string staff = _state.Staff.FirstOrDefault(r => r.Id == session.StaffId)?.Name ?? $"#{session.StaffId}";
                return $"SS-{session.Id:0000} · {room} · {staff} · {session.StartTime.ToLocalTime():dd/MM HH:mm}–{session.EndTime.ToLocalTime():HH:mm} · {session.Status}";
            }
        }

        public ReceptionistBooking AddBooking(long customerId, string customerName, string petName,
            DateTimeOffset bookingTime, IReadOnlyCollection<BookingLine> services, string note)
        {
            lock (_gate)
            {
                if (customerId <= 0 || string.IsNullOrWhiteSpace(customerName))
                    throw new BookingWorkflowException("Vui lòng chọn khách hàng hợp lệ.");
                if (string.IsNullOrWhiteSpace(petName))
                    throw new BookingWorkflowException("Vui lòng nhập tên thú cưng.");
                if (bookingTime <= DateTimeOffset.Now)
                    throw new BookingWorkflowException("Thời gian đặt lịch phải nằm trong tương lai.");
                if (services.Count == 0 || services.Any(s => s.DurationMinutes <= 0))
                    throw new BookingWorkflowException("Booking cần ít nhất một dịch vụ có thời lượng hợp lệ.");

                DateTimeOffset now = DateTimeOffset.Now;
                var booking = new ReceptionistBooking
                {
                    Id = _state.Bookings.Count == 0 ? 101 : _state.Bookings.Max(x => x.Id) + 1,
                    CustomerId = customerId,
                    CustomerName = customerName.Trim(),
                    PetName = petName.Trim(),
                    BookingTime = bookingTime,
                    DurationMinutes = services.Sum(s => s.DurationMinutes),
                    Services = Clone(services.ToList()),
                    Note = note.Trim(),
                    Status = BookingStatuses.Pending,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                WorkflowData before = Clone(_state);
                try
                {
                    _state.Bookings.Add(booking);
                    Persist();
                }
                catch
                {
                    _state = before;
                    throw;
                }
                return Clone(booking);
            }
        }

        public void Confirm(long id)
        {
            lock (_gate)
            {
                var booking = RequireBooking(id);
                if (booking.Status != BookingStatuses.Pending)
                    throw new BookingWorkflowException($"Chỉ booking PENDING mới được xác nhận. Trạng thái hiện tại: {booking.StatusLabel}.");
                WorkflowData before = Clone(_state);
                try
                {
                    booking.Status = BookingStatuses.Confirmed;
                    booking.UpdatedAt = DateTimeOffset.Now;
                    Persist();
                }
                catch
                {
                    _state = before;
                    throw;
                }
            }
        }

        public void Cancel(long id)
        {
            lock (_gate)
            {
                var booking = RequireBooking(id);
                if (booking.Status != BookingStatuses.Pending && booking.Status != BookingStatuses.Confirmed)
                    throw new BookingWorkflowException($"Không thể hủy booking ở trạng thái {booking.StatusLabel}.");
                if (_state.Sessions.Any(s => s.BookingId == id && s.Status != "CANCELLED"))
                    throw new BookingWorkflowException("Booking đã có service session. Không thể hủy khi còn ca được phân công.");
                WorkflowData before = Clone(_state);
                try
                {
                    booking.Status = BookingStatuses.Cancelled;
                    booking.UpdatedAt = DateTimeOffset.Now;
                    Persist();
                }
                catch
                {
                    _state = before;
                    throw;
                }
            }
        }

        public List<SpaRoom> GetRooms()
        {
            lock (_gate) return Clone(_state.Rooms);
        }

        public List<SpaStaff> GetStaffMembers()
        {
            lock (_gate) return Clone(_state.Staff);
        }

        public RoomStaffAvailability GetAvailability(long bookingId)
        {
            lock (_gate)
            {
                var booking = RequireBooking(bookingId);
                var start = booking.BookingTime;
                var end = start.AddMinutes(booking.DurationMinutes);
                var conflicting = _state.Sessions.Where(s => s.Status != "CANCELLED" &&
                    Overlaps(start, end, s.StartTime, s.EndTime)).ToList();
                return new RoomStaffAvailability
                {
                    Rooms = Clone(_state.Rooms.Where(r => r.IsActive && conflicting.All(s => s.RoomId != r.Id)).ToList()),
                    StaffMembers = Clone(_state.Staff.Where(r => r.IsActive && conflicting.All(s => s.StaffId != r.Id)).ToList())
                };
            }
        }

        public SpaServiceSession CreateSession(long bookingId, long roomId, long staffId)
        {
            lock (_gate)
            {
                ReceptionistBooking booking = RequireBooking(bookingId);
                if (booking.Status != BookingStatuses.Confirmed)
                    throw new BookingWorkflowException("Chỉ booking CONFIRMED mới được tạo service session.");
                if (_state.Sessions.Any(s => s.BookingId == bookingId && s.Status != "CANCELLED"))
                    throw new BookingWorkflowException("Booking này đã có service session, không thể tạo trùng.");

                SpaRoom? room = _state.Rooms.FirstOrDefault(r => r.Id == roomId && r.IsActive);
                SpaStaff? staff = _state.Staff.FirstOrDefault(r => r.Id == staffId && r.IsActive);
                if (room == null) throw new BookingWorkflowException("Phòng được chọn không tồn tại hoặc đã ngừng hoạt động.");
                if (staff == null) throw new BookingWorkflowException("Nhân viên được chọn không tồn tại hoặc đã ngừng hoạt động.");
                if (booking.DurationMinutes <= 0) throw new BookingWorkflowException("Thời lượng booking không hợp lệ.");

                DateTimeOffset start = booking.BookingTime;
                DateTimeOffset end = start.AddMinutes(booking.DurationMinutes);
                var collisions = _state.Sessions.Where(s => s.Status != "CANCELLED" &&
                    Overlaps(start, end, s.StartTime, s.EndTime)).ToList();
                var roomConflict = collisions.FirstOrDefault(s => s.RoomId == roomId);
                if (roomConflict != null)
                    throw new BookingWorkflowException($"Xung đột phòng: {room.Name} đã được sử dụng từ {roomConflict.StartTime.ToLocalTime():dd/MM HH:mm} đến {roomConflict.EndTime.ToLocalTime():HH:mm}. Vui lòng chọn phòng khác hoặc đổi lịch.");
                var staffConflict = collisions.FirstOrDefault(s => s.StaffId == staffId);
                if (staffConflict != null)
                    throw new BookingWorkflowException($"Xung đột nhân viên: {staff.Name} đã có ca từ {staffConflict.StartTime.ToLocalTime():dd/MM HH:mm} đến {staffConflict.EndTime.ToLocalTime():HH:mm}. Vui lòng chọn nhân viên khác hoặc đổi lịch.");

                var session = new SpaServiceSession
                {
                    Id = _state.Sessions.Count == 0 ? 1001 : _state.Sessions.Max(s => s.Id) + 1,
                    BookingId = bookingId,
                    RoomId = roomId,
                    StaffId = staffId,
                    StartTime = start,
                    EndTime = end,
                    CreatedAt = DateTimeOffset.Now
                };
                WorkflowData before = Clone(_state);
                try
                {
                    _state.Sessions.Add(session);
                    booking.UpdatedAt = DateTimeOffset.Now;
                    Persist();
                }
                catch
                {
                    _state = before;
                    throw;
                }
                return Clone(session);
            }
        }

        // Two appointments touching at their endpoints do not conflict.
        public static bool Overlaps(DateTimeOffset aStart, DateTimeOffset aEnd,
            DateTimeOffset bStart, DateTimeOffset bEnd) => aStart < bEnd && bStart < aEnd;

        private ReceptionistBooking RequireBooking(long id) =>
            _state.Bookings.FirstOrDefault(b => b.Id == id)
            ?? throw new BookingWorkflowException($"Không tìm thấy booking BK-{id:0000}.");

        private void Persist()
        {
            string? directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            string temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(_state, _json));
            File.Move(temp, _path, overwrite: true);
        }

        private static WorkflowData MakeDemoData()
        {
            var now = DateTimeOffset.Now;
            DateTimeOffset t(int days, int hour, int minute = 0) =>
                new(DateTime.Today.AddDays(days).AddHours(hour).AddMinutes(minute));
            ReceptionistBooking Sample(long id, string owner, string pet, DateTimeOffset time,
                int minutes, string status, string service) => new()
                {
                    Id = id, CustomerId = id - 100, CustomerName = owner, PetName = pet,
                    BookingTime = time, DurationMinutes = minutes, Status = status,
                    Services = new() { new BookingLine { ServiceName = service, DurationMinutes = minutes, UnitPrice = 250000m } },
                    Note = "Dữ liệu minh họa (offline)", CreatedAt = now, UpdatedAt = now
                };
            return new WorkflowData
            {
                Rooms = new() { new() { Id = 1, Name = "ROOM 1" }, new() { Id = 2, Name = "ROOM 2" }, new() { Id = 3, Name = "ROOM 3" } },
                Staff = new() { new() { Id = 1, Name = "Nguyen Van A" }, new() { Id = 2, Name = "Nguyen Van B" }, new() { Id = 3, Name = "Nguyen Van C" } },
                Bookings = new()
                {
                    Sample(101, "Võ Hải", "Milo", t(1, 9), 60, BookingStatuses.Pending, "Tắm vệ sinh"),
                    Sample(102, "Kha Bùi", "Bông", t(1, 10), 90, BookingStatuses.Confirmed, "Cắt tỉa lông"),
                    Sample(103, "Nguyễn An", "Miu", t(1, 10, 30), 60, BookingStatuses.Confirmed, "Spa toàn diện"),
                    Sample(104, "Lê Minh", "Cún", t(2, 14), 45, BookingStatuses.Cancelled, "Vệ sinh tai"),
                    Sample(105, "Trần Vy", "Lucky", t(-1, 9), 60, BookingStatuses.Completed, "Tắm vệ sinh"),
                    Sample(106, "Võ Hải", "Coco", t(0, 15), 60, BookingStatuses.CheckedIn, "Tắm dưỡng lông")
                },
                Sessions = new()
                {
                    new SpaServiceSession { Id = 1001, BookingId = 103, RoomId = 1, StaffId = 1,
                        StartTime = t(1, 10, 30), EndTime = t(1, 11, 30), CreatedAt = now }
                }
            };
        }
    }
}
