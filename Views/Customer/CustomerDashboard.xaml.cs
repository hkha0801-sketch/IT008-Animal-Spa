using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Animal_Spa.Views.Customer
{
    /// <summary>Chỉ hiển thị các component Customer đã có; không dùng booking lễ tân.</summary>
    public partial class CustomerDashboard : UserControl
    {
        private const int MockCustomerId = 3; // Demo only. No authenticated customer id is available yet.
        private static readonly Brush ActiveBackground = new SolidColorBrush(Color.FromRgb(64, 151, 211));

        public CustomerDashboard()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Category.SetCustomerMode();
            CustomerDetail.LoadCustomer(MockCustomerId);
            CustomerDetail.AddPetRequested += ShowAddPet;
            AddPet.CancelRequested += HideAddPet;
            Category.CategorySelected += ShowServiceCategory;
            Service.ServiceSelected += ShowServiceDetail;
            ServiceDetail.SetCustomerBrowseMode();
            ServiceDetail.CloseRequested += HideServiceDetail;
            Navigate("home");
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string page }) return;
            if (page == "catalog") BackToCategories_Click(sender, e);
            else if (page == "profile") HideAddPet();
            else Navigate(page);
        }

        private void Navigate(string page)
        {
            HomeSection.Visibility = Visibility.Collapsed;
            ProfileSection.Visibility = Visibility.Collapsed;
            CatalogSection.Visibility = Visibility.Collapsed;
            AppointmentsSection.Visibility = Visibility.Collapsed;
            PaymentsSection.Visibility = Visibility.Collapsed;

            (StackPanel section, Button nav, string title, string description) target = page switch
            {
                "profile" => (ProfileSection, ProfileNavButton, "Hồ sơ & thú cưng", "Hồ sơ minh họa; thao tác chỉnh sửa/lưu vẫn chưa kết nối hệ thống."),
                "catalog" => (CatalogSection, CatalogNavButton, "Danh mục & dịch vụ", "Duyệt và xem chi tiết; chưa hỗ trợ khách hàng tự đặt lịch."),
                "appointments" => (AppointmentsSection, AppointmentsNavButton, "Lịch hẹn của tôi", "Chưa có component theo dõi lịch hẹn của khách hàng."),
                "payments" => (PaymentsSection, PaymentsNavButton, "Thanh toán", "Chưa có component thanh toán."),
                _ => (HomeSection, HomeNavButton, "Tổng quan khách hàng", "Chỉ hiển thị những chức năng dành cho khách hàng."),
            };
            target.section.Visibility = Visibility.Visible;
            PageTitle.Text = target.title;
            PageDescription.Text = target.description;
            foreach (Button b in new[] { HomeNavButton, ProfileNavButton, CatalogNavButton,
                                         AppointmentsNavButton, PaymentsNavButton })
            {
                b.Background = b == target.nav ? ActiveBackground : Brushes.Transparent;
                b.Foreground = Brushes.White;
            }
            Dispatcher.BeginInvoke(new Action(() => ContentScroll.ScrollToTop()), DispatcherPriority.Loaded);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginViews().Show();
            Window.GetWindow(this)?.Close();
        }

        private void ShowAddPet(int customerId)
        {
            AddPet.PrepareForCustomer(customerId);
            CustomerDetail.Visibility = Visibility.Collapsed;
            AddPet.Visibility = Visibility.Visible;
            Navigate("profile");
        }

        private void HideAddPet()
        {
            AddPet.Visibility = Visibility.Collapsed;
            CustomerDetail.Visibility = Visibility.Visible;
            Navigate("profile");
        }

        private void BackToCategories_Click(object sender, RoutedEventArgs e)
        {
            Category.Visibility = Visibility.Visible;
            ServiceResults.Visibility = Visibility.Collapsed;
            ServiceDetails.Visibility = Visibility.Collapsed;
            Navigate("catalog");
        }

        private void BackToServices_Click(object sender, RoutedEventArgs e) => HideServiceDetail();

        private void ShowServiceCategory(string category)
        {
            Service.LoadCategory(category);
            Category.Visibility = Visibility.Collapsed;
            ServiceResults.Visibility = Visibility.Visible;
            ServiceDetails.Visibility = Visibility.Collapsed;
            Navigate("catalog");
        }

        private void ShowServiceDetail(string name, string description, string duration)
        {
            ServiceDetail.LoadService(name, description, duration);
            Category.Visibility = Visibility.Collapsed;
            ServiceResults.Visibility = Visibility.Collapsed;
            ServiceDetails.Visibility = Visibility.Visible;
            Navigate("catalog");
        }

        private void HideServiceDetail()
        {
            Category.Visibility = Visibility.Collapsed;
            ServiceResults.Visibility = Visibility.Visible;
            ServiceDetails.Visibility = Visibility.Collapsed;
            Navigate("catalog");
        }
    }
}
