using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Animal_Spa.Models;

namespace Animal_Spa.Components.Receptionist
{
    public partial class BookingView : UserControl
    {
        // Đại diện cho một khách hàng trong ComboBox với mã và tên hiển thị.
        public sealed class CustomerOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        // Danh sách khách hàng mẫu dùng để chọn người đặt lịch.
        private readonly List<CustomerOption> _customers = new()
        {
            new CustomerOption { Id = 1, Name = "VÕ HẢI" },
            new CustomerOption { Id = 2, Name = "VÕ HẢI" },
            new CustomerOption { Id = 3, Name = "KHA BÙI" },
            new CustomerOption { Id = 4, Name = "VÕ HẢI" },
            new CustomerOption { Id = 5, Name = "VÕ HẢI" },
            new CustomerOption { Id = 6, Name = "VÕ HẢI" }
        };

        // Phát thông báo khi yêu cầu lưu lịch, Action nhận phương thức void không tham số.
        public event Action? SaveRequested;

        // ObservableCollection thông báo cho giao diện khi thêm hoặc xóa dịch vụ trong lịch.
        public ObservableCollection<BookingServiceItem> BookingItems { get; } = new();

        // Lưu mã và tên khách hàng đang được chọn để đặt lịch.
        public int SelectedCustomerId { get; private set; } = 3;
        public string SelectedCustomerName { get; private set; } = "KHA BÙI";

        // Khởi tạo giao diện, danh sách khách hàng và phần tổng hợp lịch đặt.
        public BookingView()
        {
            InitializeComponent();
            // DataContext cho phép các Binding trong XAML truy cập thuộc tính của view này.
            DataContext = this;

            CustomerComboBox.ItemsSource = _customers;
            CustomerComboBox.SelectedValue = SelectedCustomerId;

            // Tính lại số dịch vụ và tổng tiền mỗi khi danh sách thay đổi.
            BookingItems.CollectionChanged += (_, _) => UpdateBookingSummary();
            UpdateBookingSummary();
            BookingDateInput.SelectedDate = DateTime.Today.AddDays(1);
            _workflowReady = true;
            RenderEmptyDetails();
        }

        // Chọn khách hàng theo mã và bổ sung vào ComboBox nếu chưa có trong danh sách.
        public void SetCustomer(int customerId, string customerName)
        {
            SelectedCustomerId = customerId;
            SelectedCustomerName = customerName;

            CustomerOption? option = _customers.Find(x => x.Id == customerId);
            if (option == null)
            {
                option = new CustomerOption { Id = customerId, Name = customerName };
                _customers.Add(option);
                CustomerComboBox.Items.Refresh();
            }

            CustomerComboBox.SelectedValue = customerId;
        }

        // Thêm dịch vụ vào lịch và cuộn giao diện tới dịch vụ vừa thêm.
        public void AddService(BookingServiceItem item)
        {
            BookingItems.Add(item);
            BookingItemsControl.ScrollIntoViewIfPossible(item);
        }

        // Xóa toàn bộ dịch vụ trong lịch đặt hiện tại.
        public void ClearBooking()
        {
            BookingItems.Clear();
        }

        // Cập nhật mã và tên khách hàng khi lựa chọn trong ComboBox thay đổi.
        private void CustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerComboBox.SelectedItem is CustomerOption customer)
            {
                SelectedCustomerId = customer.Id;
                SelectedCustomerName = customer.Name;
            }
        }

        // Lấy dịch vụ từ Tag của nút được nhấn và xóa khỏi lịch đặt.
        private void RemoveService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BookingServiceItem item)
            {
                BookingItems.Remove(item);
            }
        }

        // Xóa danh sách dịch vụ khi người dùng nhấn Cancel.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClearBooking();
        }

        // Kiểm tra lịch có dịch vụ rồi phát sự kiện SaveRequested khi nhấn Save.
        private void SaveBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingItems.Count == 0)
            {
                MessageBox.Show(
                    "Please choose at least one service before saving the booking.",
                    "Animal Spa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            SaveRequested?.Invoke();
        }

        // Cập nhật thông báo lịch trống, số dịch vụ và tổng tiền của lịch đặt.
        private void UpdateBookingSummary()
        {
            EmptyBookingText.Visibility = BookingItems.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            TotalServicesText.Text = $"Total services: {BookingItems.Count}";

            decimal total = 0;
            foreach (BookingServiceItem item in BookingItems)
            {
                total += item.PriceValue;
            }

            TotalPriceText.Text = FormatPrice(total);
        }

        // Định dạng tiền với dấu chấm phân cách hàng nghìn và ký hiệu đồng.
        private static string FormatPrice(decimal value)
        {
            string formatted = value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".");
            return $"{formatted}đ";
        }
    }

    // Bổ sung phương thức cuộn đến một phần tử cho ItemsControl.
    internal static class ItemsControlExtensions
    {
        // Đưa phần tử vào vùng nhìn thấy ngay khi control hiển thị của nó đã được tạo.
        public static void ScrollIntoViewIfPossible(this ItemsControl itemsControl, object item)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement element)
            {
                element.BringIntoView();
                return;
            }

            // Dispatcher xếp việc cuộn vào hàng đợi giao diện để thử lại sau khi tạo control.
            itemsControl.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement generatedElement)
                {
                    generatedElement.BringIntoView();
                }
            }));
        }
    }
}
