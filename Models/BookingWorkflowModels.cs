using System;
using System.Collections.Generic;
using System.Linq;

namespace Animal_Spa.Models
{
    // The strings match the BookingStatus values defined in openapi.yaml.
    public static class BookingStatuses
    {
        public const string Pending = "PENDING";
        public const string Confirmed = "CONFIRMED";
        public const string CheckedIn = "CHECKED_IN";
        public const string Completed = "COMPLETED";
        public const string Cancelled = "CANCELLED";
    }

    public sealed class BookingLine
    {
        public string ServiceName { get; set; } = "";
        public int DurationMinutes { get; set; }
        public decimal UnitPrice { get; set; }
        public string Summary => $"{ServiceName} · {DurationMinutes} phút · {UnitPrice:N0}đ";
    }

    public sealed class ReceptionistBooking
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string PetName { get; set; } = "";
        public DateTimeOffset BookingTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Note { get; set; } = "";
        public string Status { get; set; } = BookingStatuses.Pending;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<BookingLine> Services { get; set; } = new();
        public string BookingCode => $"BK-{Id:0000}";
        public string CustomerPetLabel => $"{CustomerName} / {PetName}";
        public string BookingTimeText => BookingTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        public string StatusLabel => Status switch
        {
            BookingStatuses.Pending => "Chờ xác nhận",
            BookingStatuses.Confirmed => "Đã xác nhận",
            BookingStatuses.CheckedIn => "Đã check-in",
            BookingStatuses.Completed => "Hoàn thành",
            BookingStatuses.Cancelled => "Đã hủy",
            _ => Status
        };
        public string DurationText => $"{DurationMinutes} phút";
        public string TotalText => $"{Services.Sum(s => s.UnitPrice):N0}đ";
        public string ServiceSummary => Services.Count == 0 ? "Chưa có dịch vụ" :
            string.Join(" · ", Services.Select(s => s.ServiceName));
    }

    public sealed class SpaRoom
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public override string ToString() => Name;
    }

    public sealed class SpaStaff
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public override string ToString() => Name;
    }

    public sealed class SpaServiceSession
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public long RoomId { get; set; }
        public long StaffId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Status { get; set; } = "ASSIGNED";
        public DateTimeOffset CreatedAt { get; set; }
    }

    public sealed class RoomStaffAvailability
    {
        public List<SpaRoom> Rooms { get; set; } = new();
        public List<SpaStaff> StaffMembers { get; set; } = new();
    }

    public sealed class BookingWorkflowException : Exception
    {
        public BookingWorkflowException(string message) : base(message) { }
    }
}
