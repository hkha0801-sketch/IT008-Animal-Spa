using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Animal_Spa.Models;

namespace Animal_Spa.Components.Receptionist
{
    public partial class CustomerListView : UserControl
    {
        // Phát mã khách hàng cần xem chi tiết, Action<int> nhận phương thức void có tham số int.
        public event Action<int>? ViewDetailRequested;
        // Phát thông báo mở form thêm khách hàng, Action nhận phương thức void không tham số.
        public event Action? AddCustomerRequested;

        // ObservableCollection thông báo cho giao diện khi danh sách khách hàng thay đổi.
        public ObservableCollection<CustomerListItem> Customers { get; } = new();
        // ICollectionView cung cấp danh sách đã lọc để hiển thị lên giao diện.
        public ICollectionView CustomersView { get; }

        // Khởi tạo dữ liệu khách hàng mẫu và thiết lập bộ lọc cho danh sách.
        public CustomerListView()
        {
            InitializeComponent();

            Customers.Add(new CustomerListItem { Id = 1, Name = "VÕ HẢI", Phone = "+84xxxxxx647", PetCount = 1 });
            Customers.Add(new CustomerListItem { Id = 2, Name = "VÕ HẢI", Phone = "+84xxxxxx647", PetCount = 1 });
            Customers.Add(new CustomerListItem { Id = 3, Name = "KHA BÙI", Phone = "+84xxxxxx647", PetCount = 3 });
            Customers.Add(new CustomerListItem { Id = 4, Name = "VÕ HẢI", Phone = "+84xxxxxx647", PetCount = 1 });
            Customers.Add(new CustomerListItem { Id = 5, Name = "VÕ HẢI", Phone = "+84xxxxxx647", PetCount = 2 });
            Customers.Add(new CustomerListItem { Id = 6, Name = "VÕ HẢI", Phone = "+84xxxxxx647", PetCount = 1 });

            // Lấy view của danh sách và gán hàm quyết định khách hàng nào được hiển thị.
            CustomersView = CollectionViewSource.GetDefaultView(Customers);
            CustomersView.Filter = FilterCustomer;

            // DataContext cho phép các Binding trong XAML truy cập thuộc tính của view này.
            DataContext = this;
            UpdateEmptyState();
        }

        // Lọc khách hàng theo tên hoặc số điện thoại với từ khóa không phân biệt chữ hoa, chữ thường.
        private bool FilterCustomer(object item)
        {
            if (item is not CustomerListItem customer)
                return false;

            string keyword = SearchTextBox?.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(keyword))
                return true;

            return customer.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || customer.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        // Cập nhật chữ gợi ý, lọc lại danh sách và kiểm tra kết quả khi từ khóa thay đổi.
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            CustomersView?.Refresh();
            UpdateEmptyState();
        }

        // Hiển thị thông báo danh sách trống khi bộ lọc không có kết quả.
        private void UpdateEmptyState()
        {
            if (CustomersView == null)
                return;

            EmptyText.Visibility = CustomersView.IsEmpty
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Phát sự kiện AddCustomerRequested khi người dùng nhấn thêm khách hàng.
        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerRequested?.Invoke();
        }

        // Lấy mã khách hàng từ Tag của nút và phát sự kiện yêu cầu xem chi tiết.
        private void ViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                ViewDetailRequested?.Invoke(customerId);
            }
        }
    }
}
