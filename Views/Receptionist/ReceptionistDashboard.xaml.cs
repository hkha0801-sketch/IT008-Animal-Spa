using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Animal_Spa.Models;
using Animal_Spa.Services;

namespace Animal_Spa.Views.Receptionist
{
    /// <summary>
    /// Shell dành cho lễ tân: dùng lại các component có thật và không hiển thị
    /// RoomStaffView nếu chưa được gọi từ một booking cụ thể.
    /// </summary>
    public partial class ReceptionistDashboard : UserControl
    {
        private long? _activeBookingId;
        private const string DefaultHeader = "Chọn chức năng ở menu bên trái để bắt đầu.";
        private static readonly Brush ActiveBackground = new SolidColorBrush(Color.FromRgb(64, 151, 211));
        private static readonly Brush NormalBackground = Brushes.Transparent;

        public ReceptionistDashboard()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

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
            Navigate("home");
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string page }) return;
            // Clicking a main menu item should open its first screen, not a hidden child view.
            switch (page)
            {
                case "customers": OpenCustomers_Click(sender, e); break;
                case "services": BackToCategories_Click(sender, e); break;
                case "bookings" when RoomStaff.Visibility == Visibility.Visible:
                    ReturnToBooking(); break;
                default: Navigate(page); break;
            }
        }

        private void Navigate(string page)
        {
            HomeSection.Visibility = Visibility.Collapsed;
            CustomersSection.Visibility = Visibility.Collapsed;
            PetsSection.Visibility = Visibility.Collapsed;
            ServicesSection.Visibility = Visibility.Collapsed;
            BookingsSection.Visibility = Visibility.Collapsed;
            ResourcesSection.Visibility = Visibility.Collapsed;
            ReportsSection.Visibility = Visibility.Collapsed;

            (StackPanel section, Button nav, string title, string description) target = page switch
            {
                "customers" => (CustomersSection, CustomersNavButton, "Khách hàng", "Danh sách, chi tiết, thêm khách hàng và thú cưng (dữ liệu mẫu)."),
                "pets" => (PetsSection, PetsNavButton, "Thú cưng", "Chưa có danh sách thú cưng độc lập."),
                "services" => (ServicesSection, ServicesNavButton, "Danh mục và dịch vụ", "Chọn danh mục → xem dịch vụ → xem chi tiết."),
                "bookings" => (BookingsSection, BookingsNavButton, "Booking & Service Session", "Tạo lịch hẹn, xác nhận/hủy booking và phân công ca dịch vụ."),
                "resources" => (ResourcesSection, ResourcesNavButton, "Phòng & nhân viên", "Chỉ phân công khi đã chọn booking hợp lệ."),
                "reports" => (ReportsSection, ReportsNavButton, "Báo cáo", "Chưa có component báo cáo."),
                _ => (HomeSection, HomeNavButton, "Tổng quan lễ tân", DefaultHeader),
            };

            target.section.Visibility = Visibility.Visible;
            PageTitle.Text = target.title;
            PageDescription.Text = target.description;
            foreach (Button b in new[] { HomeNavButton, CustomersNavButton, PetsNavButton,
                       ServicesNavButton, BookingsNavButton, ResourcesNavButton, ReportsNavButton })
            {
                b.Background = b == target.nav ? ActiveBackground : NormalBackground;
                b.Foreground = Brushes.White;
            }
            ScrollTop();
        }

        private void ScrollTop() => Dispatcher.BeginInvoke(
            new Action(() => ContentScroll.ScrollToTop()), DispatcherPriority.Loaded);

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginViews().Show();
            Window.GetWindow(this)?.Close();
        }

        private void OpenCustomers_Click(object sender, RoutedEventArgs e)
        {
            CustomerList.Visibility = Visibility.Visible;
            CustomerDetail.Visibility = Visibility.Collapsed;
            AddCustomer.Visibility = Visibility.Collapsed;
            AddPet.Visibility = Visibility.Collapsed;
            Navigate("customers");
        }

        private void ShowAddCustomer()
        {
            CustomerList.Visibility = Visibility.Collapsed;
            CustomerDetail.Visibility = Visibility.Collapsed;
            AddPet.Visibility = Visibility.Collapsed;
            AddCustomer.Visibility = Visibility.Visible;
            Navigate("customers");
        }

        private void HideAddCustomer() => OpenCustomers_Click(this, new RoutedEventArgs());

        private void ShowCustomerDetail(int customerId)
        {
            CustomerDetail.LoadCustomer(customerId);
            Booking.SetCustomer(customerId, CustomerDetail.CurrentCustomerName);
            CustomerList.Visibility = Visibility.Collapsed;
            AddCustomer.Visibility = Visibility.Collapsed;
            AddPet.Visibility = Visibility.Collapsed;
            CustomerDetail.Visibility = Visibility.Visible;
            Navigate("customers");
        }

        private void ShowAddPet(int customerId)
        {
            AddPet.PrepareForCustomer(customerId);
            CustomerList.Visibility = Visibility.Collapsed;
            CustomerDetail.Visibility = Visibility.Collapsed;
            AddCustomer.Visibility = Visibility.Collapsed;
            AddPet.Visibility = Visibility.Visible;
            Navigate("customers");
        }

        private void HideAddPet()
        {
            AddPet.Visibility = Visibility.Collapsed;
            CustomerDetail.Visibility = Visibility.Visible;
            Navigate("customers");
        }

        private void BackToCategories_Click(object sender, RoutedEventArgs e)
        {
            Category.Visibility = Visibility.Visible;
            ServiceResults.Visibility = Visibility.Collapsed;
            ServiceDetails.Visibility = Visibility.Collapsed;
            Navigate("services");
        }

        private void BackToServices_Click(object sender, RoutedEventArgs e) => HideServiceDetail();

        private void ShowServiceCategory(string category)
        {
            Service.LoadCategory(category);
            Category.Visibility = Visibility.Collapsed;
            ServiceResults.Visibility = Visibility.Visible;
            ServiceDetails.Visibility = Visibility.Collapsed;
            Navigate("services");
        }

        private void ShowServiceDetail(string name, string description, string duration)
        {
            ServiceDetail.LoadService(name, description, duration);
            Category.Visibility = Visibility.Collapsed;
            ServiceResults.Visibility = Visibility.Collapsed;
            ServiceDetails.Visibility = Visibility.Visible;
            Navigate("services");
        }

        private void HideServiceDetail()
        {
            Category.Visibility = Visibility.Collapsed;
            ServiceResults.Visibility = Visibility.Visible;
            ServiceDetails.Visibility = Visibility.Collapsed;
            Navigate("services");
        }

        private void AddServiceToBooking(BookingServiceItem item)
        {
            Booking.AddService(item);
            Booking.ShowCreateTab();
            RoomStaff.Visibility = Visibility.Collapsed;
            Booking.Visibility = Visibility.Visible;
            Navigate("bookings");
        }

        private void OpenBookingManagement_Click(object sender, RoutedEventArgs e)
        {
            Booking.Visibility = Visibility.Visible;
            RoomStaff.Visibility = Visibility.Collapsed;
            Booking.ShowManagementTab();
            Navigate("bookings");
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
                    Booking.SelectedCustomerId, Booking.SelectedCustomerName,
                    pet, start, services, note);

                Booking.ClearBooking();
                Booking.ClearSchedule();
                ShowRoomStaff(created.Id);
            }
            catch (Exception ex) when (ex is BookingWorkflowException ||
                ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show(ex.Message, "Không thể tạo booking",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowRoomStaff(long bookingId)
        {
            try
            {
                // Prepare first: a failure must not hide BookingView.
                RoomStaff.PrepareBooking(bookingId);
                _activeBookingId = bookingId;
                Booking.Visibility = Visibility.Collapsed;
                RoomStaff.Visibility = Visibility.Visible;
                Navigate("bookings");
            }
            catch (Exception ex) when (ex is BookingWorkflowException ||
                ex is IOException || ex is UnauthorizedAccessException)
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
            _activeBookingId = null;
            Navigate("bookings");
        }

        private void FinishSession(long bookingId)
        {
            RoomStaff.Visibility = Visibility.Collapsed;
            Booking.Visibility = Visibility.Visible;
            Booking.ShowBooking(bookingId);
            _activeBookingId = null;
            Navigate("bookings");
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
    }
}
