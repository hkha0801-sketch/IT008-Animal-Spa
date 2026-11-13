using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Animal_Spa.Models;
using Animal_Spa.Services;

namespace Animal_Spa.Components.Receptionist
{
    public partial class RoomStaffView : UserControl
    {
        private readonly Brush _selectedBorder = new SolidColorBrush(Color.FromRgb(62, 132, 216));
        private readonly Brush _normalBorder = Brushes.Transparent;
        private readonly Brush _selectedDot = new SolidColorBrush(Color.FromRgb(168, 201, 232));
        private long? _bookingId;
        private long? _selectedRoomId;
        private long? _selectedStaffId;
        private List<SpaRoom> _rooms = new();
        private List<SpaStaff> _staff = new();
        private bool _busy;

        public event Action? CancelRequested;
        // Fires only after a real service session has been persisted.
        public event Action<long>? Confirmed;

        public string SelectedRoom { get; private set; } = "";
        public string SelectedStaff { get; private set; } = "";

        public RoomStaffView() => InitializeComponent();

        public void PrepareBooking(long bookingId)
        {
            _bookingId = bookingId;
            _selectedRoomId = _selectedStaffId = null;
            SelectedRoom = SelectedStaff = "";
            RoomStaffFeedbackText.Text = "";
            ResetRooms();
            ResetStaff();
            RefreshAvailability();
        }

        private void RefreshAvailability()
        {
            if (!_bookingId.HasValue) return;
            try
            {
                var db = BookingWorkflowStore.Current;
                ReceptionistBooking booking = db.GetBooking(_bookingId.Value);
                SpaServiceSession? existing = db.GetSession(booking.Id);
                RoomStaffAvailability free = db.GetAvailability(booking.Id);
                _rooms = db.GetRooms();
                _staff = db.GetStaffMembers();
                bool assignable = booking.Status == BookingStatuses.Confirmed && existing == null;

                BookingContextText.Text = $"{booking.BookingCode} · {booking.PetName} · " +
                    $"{booking.BookingTime.ToLocalTime():dd/MM/yyyy HH:mm}–{booking.BookingTime.AddMinutes(booking.DurationMinutes).ToLocalTime():HH:mm} · {booking.Status}";
                ConfirmBookingButton.IsEnabled = !_busy && booking.Status == BookingStatuses.Pending;

                UpdateRoom(Room1Button, Room1StatusDot, Room1StatusText, "ROOM 1", free.Rooms, assignable);
                UpdateRoom(Room2Button, Room2StatusDot, Room2StatusText, "ROOM 2", free.Rooms, assignable);
                UpdateRoom(Room3Button, Room3StatusDot, Room3StatusText, "ROOM 3", free.Rooms, assignable);
                UpdateStaff(StaffAButton, StaffAStatusDot, StaffAStatusText, "Nguyen Van A", free.StaffMembers, assignable);
                UpdateStaff(StaffBButton, StaffBStatusDot, StaffBStatusText, "Nguyen Van B", free.StaffMembers, assignable);
                UpdateStaff(StaffCButton, StaffCStatusDot, StaffCStatusText, "Nguyen Van C", free.StaffMembers, assignable);

                if (!assignable)
                {
                    _selectedRoomId = _selectedStaffId = null;
                    SelectedRoom = SelectedStaff = "";
                    ResetRooms(); ResetStaff();
                }
                else
                {
                    if (!_selectedRoomId.HasValue || !free.Rooms.Any(r => r.Id == _selectedRoomId))
                    { _selectedRoomId = null; SelectedRoom = ""; ResetRooms(); }
                    if (!_selectedStaffId.HasValue || !free.StaffMembers.Any(s => s.Id == _selectedStaffId))
                    { _selectedStaffId = null; SelectedStaff = ""; ResetStaff(); }
                }
                SelectedRoomText.Text = string.IsNullOrEmpty(SelectedRoom) ? "Chưa chọn" : SelectedRoom.Replace("ROOM ", "");
                SelectedStaffText.Text = string.IsNullOrEmpty(SelectedStaff) ? "Chưa chọn" : SelectedStaff;
                SaveSessionButton.IsEnabled = !_busy && assignable && _selectedRoomId.HasValue && _selectedStaffId.HasValue;
                if (existing != null)
                    ShowFeedback("Booking đã có service session. Không thể phân công lần nữa.", false);
                else if (booking.Status == BookingStatuses.Pending)
                    ShowFeedback("Hãy xác nhận booking trước khi chọn phòng và nhân viên.", false);
                else if (booking.Status != BookingStatuses.Confirmed)
                    ShowFeedback("Trạng thái booking không cho phép tạo service session.", true);
                else if (free.Rooms.Count == 0 || free.StaffMembers.Count == 0)
                    ShowFeedback("Không đủ phòng hoặc nhân viên trống trong khung giờ. Hãy đổi lịch.", true);
                else if (_selectedRoomId == null || _selectedStaffId == null)
                    ShowFeedback("Chọn một phòng và một nhân viên có trạng thái Available.", false);
            }
            catch (Exception ex) when (ex is BookingWorkflowException || ex is IOException || ex is UnauthorizedAccessException)
            {
                ConfirmBookingButton.IsEnabled = SaveSessionButton.IsEnabled = false;
                foreach (var b in new[] {Room1Button, Room2Button, Room3Button, StaffAButton, StaffBButton, StaffCButton}) b.IsEnabled = false;
                ShowFeedback(ex.Message, true);
            }
        }

        private void UpdateRoom(Button button, Ellipse dot, TextBlock status, string name, List<SpaRoom> free, bool assignable)
        {
            SpaRoom? item = _rooms.FirstOrDefault(r => r.Name == name);
            bool available = item != null && item.IsActive && free.Any(r => r.Id == item.Id);
            button.IsEnabled = !_busy && assignable && available;
            dot.Fill = available ? Brushes.LimeGreen : Brushes.Red;
            status.Text = available ? "Available" : "Busy";
        }

        private void UpdateStaff(Button button, Ellipse dot, TextBlock status, string name, List<SpaStaff> free, bool assignable)
        {
            SpaStaff? item = _staff.FirstOrDefault(r => r.Name == name);
            bool available = item != null && item.IsActive && free.Any(r => r.Id == item.Id);
            button.IsEnabled = !_busy && assignable && available;
            dot.Fill = available ? Brushes.LimeGreen : Brushes.Red;
            status.Text = available ? "Available" : "Busy";
        }

        private void Room_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { IsEnabled: true, Tag: string name } button) return;
            SpaRoom? room = _rooms.FirstOrDefault(r => r.Name == name);
            if (room == null) return;
            _selectedRoomId = room.Id;
            SelectedRoom = name;
            if (button == Room1Button) SelectRoom(button, Room1Outer, Room1Inner, name);
            else if (button == Room2Button) SelectRoom(button, Room2Outer, Room2Inner, name);
            else if (button == Room3Button) SelectRoom(button, Room3Outer, Room3Inner, name);
            RefreshAvailability();
        }

        private void Staff_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { IsEnabled: true, Tag: string name } button) return;
            SpaStaff? staff = _staff.FirstOrDefault(r => r.Name == name);
            if (staff == null) return;
            _selectedStaffId = staff.Id;
            SelectedStaff = name;
            if (button == StaffAButton) SelectStaff(button, StaffAOuter, StaffAInner, name);
            else if (button == StaffBButton) SelectStaff(button, StaffBOuter, StaffBInner, name);
            else if (button == StaffCButton) SelectStaff(button, StaffCOuter, StaffCInner, name);
            RefreshAvailability();
        }

        private void SelectRoom(Button button, Ellipse outer, Ellipse inner, string name)
        {
            ResetRooms();
            button.BorderBrush = outer.Stroke = _selectedBorder;
            inner.Fill = _selectedDot;
            SelectedRoom = name;
            SelectedRoomText.Text = name.Replace("ROOM ", "");
        }
        private void SelectStaff(Button button, Ellipse outer, Ellipse inner, string name)
        {
            ResetStaff();
            button.BorderBrush = outer.Stroke = _selectedBorder;
            inner.Fill = _selectedDot;
            SelectedStaff = name;
            SelectedStaffText.Text = name;
        }
        private void ResetRooms()
        {
            ResetChoice(Room1Button, Room1Outer, Room1Inner);
            ResetChoice(Room2Button, Room2Outer, Room2Inner);
            ResetChoice(Room3Button, Room3Outer, Room3Inner);
        }
        private void ResetStaff()
        {
            ResetChoice(StaffAButton, StaffAOuter, StaffAInner);
            ResetChoice(StaffBButton, StaffBOuter, StaffBInner);
            ResetChoice(StaffCButton, StaffCOuter, StaffCInner);
        }
        private void ResetChoice(Button button, Ellipse outer, Ellipse inner)
        {
            button.BorderBrush = _normalBorder;
            outer.Stroke = Brushes.Black;
            inner.Fill = Brushes.Transparent;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => CancelRequested?.Invoke();

        private void ConfirmBooking_Click(object sender, RoutedEventArgs e)
        {
            if (!_bookingId.HasValue || !ConfirmBookingButton.IsEnabled) return;
            long id = _bookingId.Value;
            if (MessageBox.Show("Xác nhận booking để tiếp tục phân công phòng và nhân viên?", "Animal Spa",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            _busy = true;
            try
            {
                BookingWorkflowStore.Current.Confirm(id);
                ShowFeedback("Đã xác nhận booking. Hãy chọn phòng và nhân viên.", false);
            }
            catch (Exception ex) when (ex is BookingWorkflowException || ex is IOException || ex is UnauthorizedAccessException)
            { ShowFeedback(ex.Message, true); }
            finally { _busy = false; RefreshAvailability(); }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!_bookingId.HasValue || !_selectedRoomId.HasValue || !_selectedStaffId.HasValue || !SaveSessionButton.IsEnabled) return;
            long bookingId = _bookingId.Value;
            _busy = true;
            try
            {
                SpaServiceSession session = BookingWorkflowStore.Current.CreateSession(
                    bookingId, _selectedRoomId.Value, _selectedStaffId.Value);
                ShowFeedback($"Đã lưu SS-{session.Id:0000}: {SelectedRoom} / {SelectedStaff}.", false);
                MessageBox.Show($"Đã tạo service session SS-{session.Id:0000} thành công.",
                    "Animal Spa", MessageBoxButton.OK, MessageBoxImage.Information);
                Confirmed?.Invoke(bookingId);
            }
            catch (Exception ex) when (ex is BookingWorkflowException || ex is IOException || ex is UnauthorizedAccessException)
            { ShowFeedback("Không thể tạo session: " + ex.Message, true); }
            finally { _busy = false; RefreshAvailability(); }
        }

        private void ShowFeedback(string message, bool error)
        {
            RoomStaffFeedbackText.Text = message;
            RoomStaffFeedbackText.Foreground = error ? Brushes.Firebrick : Brushes.DarkSlateBlue;
        }
    }
}
