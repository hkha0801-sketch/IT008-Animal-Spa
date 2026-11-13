using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Animal_Spa.Models;
using Animal_Spa.Services;

namespace Animal_Spa.Components.Receptionist
{
    /// <summary>
    /// Booking list/details/confirm/cancel are a tab inside BookingView.
    /// The RoomStaffView component owns the entire room/staff assignment UI.
    /// </summary>
    public partial class BookingView
    {
        public ObservableCollection<ReceptionistBooking> FilteredBookings { get; } = new();
        public event Action<long>? AssignSessionRequested;

        private List<ReceptionistBooking> _allBookings = new();
        private ReceptionistBooking? _selectedBooking;
        private bool _workflowReady;
        private bool _suppressBookingSelection;
        private bool _workflowBusy;

        public bool TryGetSchedule(out string petName, out DateTimeOffset start, out string note, out string error)
        {
            petName = PetNameInput.Text.Trim();
            start = default;
            note = BookingNoteInput.Text.Trim();
            error = "";
            if (petName.Length == 0)
            {
                error = "Vui lòng nhập tên thú cưng trong phần thông tin lịch hẹn.";
                return false;
            }
            if (BookingDateInput.SelectedDate is not DateTime date ||
                !TimeSpan.TryParseExact(BookingTimeInput.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture,
                    out TimeSpan time))
            {
                error = "Vui lòng nhập ngày hẹn và giờ HH:mm hợp lệ, ví dụ 09:30.";
                return false;
            }
            start = new DateTimeOffset(DateTime.SpecifyKind(date.Date + time, DateTimeKind.Local));
            if (start <= DateTimeOffset.Now)
            {
                error = "Giờ hẹn phải nằm trong tương lai.";
                return false;
            }
            return true;
        }

        public void ClearSchedule()
        {
            PetNameInput.Clear();
            BookingNoteInput.Clear();
            BookingDateInput.SelectedDate = DateTime.Today.AddDays(1);
            BookingTimeInput.Text = "09:00";
        }

        // When a booking is created or assigned, show it in the existing BookingView tab.
        public void ShowBooking(long id)
        {
            BookingTabs.SelectedIndex = 1;
            SearchBox.Text = "";
            StatusFilter.SelectedIndex = 0;
            RefreshBookings(id);
            BringIntoView();
        }

        private void BookingWorkflow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_workflowReady) return;
            RefreshBookings(_selectedBooking?.Id);
        }

        private void BookingManagement_Loaded(object sender, RoutedEventArgs e)
        {
            if (_workflowReady) RefreshBookings(_selectedBooking?.Id);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshBookings(_selectedBooking?.Id);

        private void RefreshBookings(long? selectedId = null)
        {
            if (!_workflowReady) return;
            try
            {
                _allBookings = BookingWorkflowStore.Current.GetBookings();
                ApplyFilter(selectedId ?? _selectedBooking?.Id);
            }
            catch (Exception ex) when (ex is BookingWorkflowException || ex is IOException || ex is UnauthorizedAccessException)
            {
                _allBookings = new();
                ApplyFilter(null);
                ShowFeedback("Không thể đọc danh sách booking: " + ex.Message, true);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_workflowReady) ApplyFilter(_selectedBooking?.Id);
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_workflowReady) ApplyFilter(_selectedBooking?.Id);
        }

        private void ApplyFilter(long? selectedId)
        {
            string keyword = Fold(SearchBox.Text.Trim());
            string status = (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "ALL";
            var result = _allBookings.Where(b =>
                (status == "ALL" || b.Status == status) &&
                (keyword.Length == 0 || Fold($"{b.BookingCode} {b.Id} {b.CustomerName} {b.PetName} {b.ServiceSummary}")
                    .Contains(keyword, StringComparison.Ordinal))).ToList();
            _suppressBookingSelection = true;
            FilteredBookings.Clear();
            foreach (var b in result) FilteredBookings.Add(b);
            BookingTable.SelectedItem = selectedId.HasValue
                ? result.FirstOrDefault(b => b.Id == selectedId.Value) : null;
            _suppressBookingSelection = false;
            CountText.Text = $"{result.Count} / {_allBookings.Count} booking";
            if (BookingTable.SelectedItem is ReceptionistBooking selected) LoadBookingDetails(selected.Id);
            else RenderEmptyDetails();
        }

        private static string Fold(string input)
        {
            string normalized = input.ToLowerInvariant().Replace('đ','d').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private void BookingTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_workflowReady || _suppressBookingSelection) return;
            if (BookingTable.SelectedItem is ReceptionistBooking selected) LoadBookingDetails(selected.Id);
            else RenderEmptyDetails();
        }

        private void LoadBookingDetails(long id)
        {
            try
            {
                var booking = BookingWorkflowStore.Current.GetBooking(id);
                _selectedBooking = booking;
                DetailCode.Text = booking.BookingCode;
                DetailStatus.Text = $"Trạng thái: {booking.StatusLabel} ({booking.Status})";
                DetailStatus.Foreground = booking.Status switch
                {
                    BookingStatuses.Pending => Brushes.DarkGoldenrod,
                    BookingStatuses.Confirmed => Brushes.SeaGreen,
                    BookingStatuses.Cancelled => Brushes.Firebrick,
                    _ => Brushes.SteelBlue
                };
                DetailCustomer.Text = $"Khách hàng: {booking.CustomerName} (#{booking.CustomerId})";
                DetailPet.Text = "Thú cưng: " + booking.PetName;
                DetailTime.Text = "Lịch hẹn: " + booking.BookingTimeText;
                DetailDuration.Text = $"Thời lượng: {booking.DurationText} · Tổng: {booking.TotalText}";
                DetailServices.Text = "Dịch vụ: " + booking.ServiceSummary;
                DetailNote.Text = string.IsNullOrWhiteSpace(booking.Note)
                    ? "Ghi chú: (không có)" : "Ghi chú: " + booking.Note;

                var session = BookingWorkflowStore.Current.GetSession(id);
                ConfirmButton.IsEnabled = !_workflowBusy && booking.Status == BookingStatuses.Pending;
                CancelButton.IsEnabled = !_workflowBusy && session == null &&
                    (booking.Status == BookingStatuses.Pending || booking.Status == BookingStatuses.Confirmed);
                AssignButton.IsEnabled = !_workflowBusy && session == null && booking.Status == BookingStatuses.Confirmed;
                SessionText.Text = BookingWorkflowStore.Current.GetSessionSummary(id);
                if (booking.Status == BookingStatuses.Confirmed && session == null)
                {
                    var availability = BookingWorkflowStore.Current.GetAvailability(id);
                    AvailabilityText.Text = $"Còn {availability.Rooms.Count} phòng và {availability.StaffMembers.Count} nhân viên trống trong khung giờ đã chọn.";
                    if (availability.Rooms.Count == 0 || availability.StaffMembers.Count == 0)
                    {
                        AssignButton.IsEnabled = false;
                        AvailabilityText.Text += " Không đủ tài nguyên. Cần đổi lịch hoặc giải phóng ca trùng.";
                    }
                }
                else AvailabilityText.Text = session != null
                    ? "Booking đã được phân công, không thể tạo thêm session."
                    : "Chỉ booking CONFIRMED mới được phân công.";
            }
            catch (Exception ex) when (ex is BookingWorkflowException || ex is IOException || ex is UnauthorizedAccessException)
            {
                RenderEmptyDetails();
                ShowFeedback(ex.Message, true);
            }
        }

        private void RenderEmptyDetails()
        {
            _selectedBooking = null;
            DetailCode.Text = "Chọn một booking trong danh sách";
            DetailStatus.Text = DetailCustomer.Text = DetailPet.Text = DetailTime.Text = "";
            DetailDuration.Text = DetailServices.Text = DetailNote.Text = "";
            SessionText.Text = "Chưa chọn booking.";
            AvailabilityText.Text = "";
            ConfirmButton.IsEnabled = CancelButton.IsEnabled = AssignButton.IsEnabled = false;
        }

        private void BookingConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBooking?.Status != BookingStatuses.Pending || !ConfirmButton.IsEnabled) return;
            long id = _selectedBooking.Id;
            if (MessageBox.Show($"Xác nhận booking {_selectedBooking.BookingCode}?", "Animal Spa",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            RunWorkflowAction(() => BookingWorkflowStore.Current.Confirm(id), "Booking đã được xác nhận.");
        }

        private void BookingCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBooking == null || !CancelButton.IsEnabled) return;
            long id = _selectedBooking.Id;
            if (MessageBox.Show($"Bạn có chắc muốn hủy {_selectedBooking.BookingCode}?", "Animal Spa",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            RunWorkflowAction(() => BookingWorkflowStore.Current.Cancel(id), "Booking đã được hủy.");
        }

        private void BookingAssign_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBooking?.Status != BookingStatuses.Confirmed || !AssignButton.IsEnabled) return;
            AssignSessionRequested?.Invoke(_selectedBooking.Id);
        }

        private void RunWorkflowAction(Action action, string success)
        {
            if (_workflowBusy) return;
            long? id = _selectedBooking?.Id;
            _workflowBusy = true;
            ConfirmButton.IsEnabled = CancelButton.IsEnabled = AssignButton.IsEnabled = false;
            try
            {
                action();
                RefreshBookings(id);
                ShowFeedback(success, false);
            }
            catch (Exception ex) when (ex is BookingWorkflowException || ex is IOException || ex is UnauthorizedAccessException)
            {
                RefreshBookings(id);
                ShowFeedback(ex.Message, true);
            }
            finally
            {
                _workflowBusy = false;
                if (_selectedBooking != null) LoadBookingDetails(_selectedBooking.Id);
            }
        }

        private void ShowFeedback(string message, bool error)
        {
            FeedbackText.Text = (error ? "Lỗi: " : "✓ ") + message;
            FeedbackText.Foreground = error ? Brushes.Firebrick : Brushes.SeaGreen;
        }
    }
}
