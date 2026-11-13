using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using Animal_Spa.Models;
using Animal_Spa.Services;

namespace Animal_Spa.Views.Receptionist
{
    public partial class ReceptionistDashboard : UserControl
    {
        private long? _activeBookingId;
        public ReceptionistDashboard()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            CustomerList.ViewDetailRequested += ShowCustomerDetail;
            CustomerList.AddCustomerRequested += ShowAddCustomer;

            AddCustomer.CancelRequested += HideAddCustomer;
            CustomerDetail.AddPetRequested += ShowAddPet;
            AddPet.CancelRequested += HideAddPet;
            Category.CategorySelected += ShowServiceCategory;
            Service.ServiceSelected += ShowServiceDetail;
            ServiceDetail.ServiceChosen += AddServiceToBooking;
            ServiceDetail.CloseRequested += HideServiceDetail;
            Booking.SaveRequested += CreateBookingFromCart;
            Booking.AssignSessionRequested += ShowRoomStaff;
            RoomStaff.CancelRequested += ReturnToBooking;
            RoomStaff.Confirmed += FinishSession;

        }

        private void ShowAddCustomer()
        {
            CustomerDetail.Visibility = Visibility.Collapsed;
            AddPet.Visibility = Visibility.Collapsed;
            AddCustomer.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(
                new Action(() => AddCustomer.BringIntoView()),
                DispatcherPriority.Loaded);
        }

        private void HideAddCustomer()
        {
            AddCustomer.Visibility = Visibility.Collapsed;

            Dispatcher.BeginInvoke(
                new Action(() => CustomerList.BringIntoView()),
                DispatcherPriority.Loaded);
        }

        private void ShowCustomerDetail(int customerId)
        {
            AddCustomer.Visibility = Visibility.Collapsed;
            AddPet.Visibility = Visibility.Collapsed;

            CustomerDetail.LoadCustomer(customerId);
            Booking.SetCustomer(customerId, CustomerDetail.CurrentCustomerName);
            CustomerDetail.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(
                new Action(() => CustomerDetail.BringIntoView()),
                DispatcherPriority.Loaded);
        }

        private void ShowAddPet(int customerId)
        {
            AddCustomer.Visibility = Visibility.Collapsed;
            CustomerDetail.Visibility = Visibility.Visible;

            AddPet.PrepareForCustomer(customerId);
            AddPet.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(
                new Action(() => AddPet.BringIntoView()),
                DispatcherPriority.Loaded);
        }

        private void ShowServiceCategory(string category)
        {
            ServiceDetail.Visibility = Visibility.Collapsed;
            Service.LoadCategory(category);
            Service.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(
                new Action(() => Service.BringIntoView()),
                DispatcherPriority.Loaded);
        }

        private void ShowServiceDetail(string name, string description, string duration)
        {
            ServiceDetail.LoadService(name, description, duration);
            ServiceDetail.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(
                new Action(() => ServiceDetail.BringIntoView()),
                DispatcherPriority.Loaded);
        }


        private void AddServiceToBooking(Animal_Spa.Models.BookingServiceItem item)
        {
            Booking.AddService(item);
            ServiceDetail.Visibility = Visibility.Collapsed;

            Dispatcher.BeginInvoke(
                new Action(() => Booking.BringIntoView()),
                DispatcherPriority.Loaded);
        }


        private void CreateBookingFromCart()
        {
            if (Booking.BookingItems.Count == 0) return;
            if (!Booking.TryGetSchedule(out string pet, out DateTimeOffset start,
                    out string note, out string error))
            {
                MessageBox.Show(error, "Thông tin lịch hẹn chưa hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var services = Booking.BookingItems.Select(item => new BookingLine
                {
                    ServiceName = item.ServiceName,
                    DurationMinutes = ParseDuration(item.Duration),
                    UnitPrice = item.PriceValue
                }).ToList();
                ReceptionistBooking created = BookingWorkflowStore.Current.AddBooking(
                    Booking.SelectedCustomerId,
                    Booking.SelectedCustomerName,
                    pet, start, services, note);

                Booking.ClearBooking();
                Booking.ClearSchedule();
                // Booking is persisted as PENDING; RoomStaff asks for explicit confirmation.
                ShowRoomStaff(created.Id);
            }
            catch (Exception ex) when (ex is BookingWorkflowException ||
                ex is System.IO.IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show(ex.Message, "Không thể tạo booking",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowRoomStaff(long bookingId)
        {
            try
            {
                _activeBookingId = bookingId;
                RoomStaff.PrepareBooking(bookingId);
                Booking.Visibility = Visibility.Collapsed;
                RoomStaff.Visibility = Visibility.Visible;
                Dispatcher.BeginInvoke(new Action(() => RoomStaff.BringIntoView()), DispatcherPriority.Loaded);
            }
            catch (Exception ex) when (ex is BookingWorkflowException ||
                ex is System.IO.IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show(ex.Message, "Không mở được ROOM & STAFF",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReturnToBooking()
        {
            RoomStaff.Visibility = Visibility.Collapsed;
            Booking.Visibility = Visibility.Visible;
            if (_activeBookingId.HasValue) Booking.ShowBooking(_activeBookingId.Value);
            Dispatcher.BeginInvoke(new Action(() => Booking.BringIntoView()), DispatcherPriority.Loaded);
        }

        private void FinishSession(long bookingId)
        {
            RoomStaff.Visibility = Visibility.Collapsed;
            Booking.Visibility = Visibility.Visible;
            Booking.ShowBooking(bookingId);
            Dispatcher.BeginInvoke(new Action(() => Booking.BringIntoView()), DispatcherPriority.Loaded);
        }

        private static int ParseDuration(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                text ?? "", @"^\s*(\d+)\s*mins?\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out int minutes) || minutes <= 0)
                throw new BookingWorkflowException($"Thời lượng dịch vụ không hợp lệ: {text}");
            return minutes;
        }

        private void HideServiceDetail()
        {
            ServiceDetail.Visibility = Visibility.Collapsed;

            Dispatcher.BeginInvoke(
                new Action(() => Service.BringIntoView()),
                DispatcherPriority.Loaded);
        }

        private void HideAddPet()
        {
            AddPet.Visibility = Visibility.Collapsed;

            Dispatcher.BeginInvoke(
                new Action(() => CustomerDetail.BringIntoView()),
                DispatcherPriority.Loaded);
        }
    }
}
