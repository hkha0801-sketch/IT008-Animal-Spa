using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Animal_Spa.Views.Staff
{
    /// <summary>Không hiển thị hoặc chỉnh sửa booking; các module Staff chưa tồn tại.</summary>
    public partial class StaffDashboard : UserControl
    {
        private static readonly Brush ActiveBackground = new SolidColorBrush(Color.FromRgb(64, 151, 211));

        public StaffDashboard()
        {
            InitializeComponent();
            Navigate("home");
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string page) Navigate(page);
        }

        private void Navigate(string page)
        {
            HomeSection.Visibility = Visibility.Collapsed;
            SessionsSection.Visibility = Visibility.Collapsed;
            ScheduleSection.Visibility = Visibility.Collapsed;
            StaffProfileSection.Visibility = Visibility.Collapsed;

            (StackPanel section, Button nav, string title, string description) target = page switch
            {
                "sessions" => (SessionsSection, SessionsNavButton, "Ca dịch vụ", "Chưa có component xử lý ca dịch vụ được phân công."),
                "schedule" => (ScheduleSection, ScheduleNavButton, "Lịch làm việc", "Chưa có component lịch làm việc."),
                "staffprofile" => (StaffProfileSection, StaffProfileNavButton, "Hồ sơ cá nhân", "Chưa có component hồ sơ nhân viên."),
                _ => (HomeSection, HomeNavButton, "Tổng quan nhân viên", "Chức năng Staff chưa triển khai; các vùng còn lại để trống."),
            };
            target.section.Visibility = Visibility.Visible;
            PageTitle.Text = target.title;
            PageDescription.Text = target.description;
            foreach (Button b in new[] { HomeNavButton, SessionsNavButton, ScheduleNavButton, StaffProfileNavButton })
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
    }
}
