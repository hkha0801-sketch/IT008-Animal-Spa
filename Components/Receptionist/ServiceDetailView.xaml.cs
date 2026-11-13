using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Animal_Spa.Models;
using System.Linq;

namespace Animal_Spa.Components.Receptionist
{
    public partial class ServiceDetailView : UserControl
    {
        // Các Brush quy định màu nền và viền của ô giá theo trạng thái chọn.
        private readonly Brush _normalCellBackground =
            new SolidColorBrush(Color.FromRgb(168, 201, 232));

        private readonly Brush _selectedCellBackground =
            new SolidColorBrush(Color.FromRgb(78, 166, 232));

        private readonly Brush _selectedBorder =
            new SolidColorBrush(Color.FromRgb(57, 126, 214));

        // Dictionary lưu giá theo khóa ghép từ loại thú cưng và nhóm cân nặng.
        private readonly Dictionary<string, string> _currentPrices = new();
        // Lưu loại thú cưng, nhóm cân nặng và giá của ô đang được chọn.
        private string _selectedAnimalType = string.Empty;
        private string _selectedWeight = string.Empty;
        private string _selectedPrice = string.Empty;

        // Phát thông báo đóng phần chi tiết, Action nhận phương thức void không tham số.
        public event Action? CloseRequested;
        // Phát dịch vụ đã chọn, Action<BookingServiceItem> nhận phương thức void có tham số dịch vụ.
        public event Action<BookingServiceItem>? ServiceChosen;

        // Khởi tạo giao diện, bảng giá mặc định và trạng thái chưa chọn dịch vụ.
        public ServiceDetailView()
        {
            InitializeComponent();

            // Bỏ qua việc thiết lập dữ liệu và tải ảnh khi đang mở trong trình thiết kế.
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            SetDefaultPricing();
            ResetSelection();
        }

        // Chế độ Customer chỉ xem bảng giá; không được tạo booking qua chức năng Receptionist.
        public void SetCustomerBrowseMode()
        {
            ChooseButton.Visibility = Visibility.Collapsed;
        }

        // Hiển thị thông tin dịch vụ, áp dụng bảng giá và xóa lựa chọn trước đó.
        public void LoadService(string name, string description, string duration)
        {
            ServiceNameText.Text = name;
            DescriptionText.Text = description;
            DurationText.Text = duration;

            ApplyPricingForService(name);
            ResetSelection();
        }

        // Chọn ô giá và cập nhật biểu tượng, cân nặng cùng giá dịch vụ trên giao diện.
        private void PriceCell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string tag)
            {
                return;
            }

            // Tag chứa loại thú cưng và nhóm cân nặng được ngăn cách bằng ký tự |.
            string[] parts = tag.Split('|');
            if (parts.Length != 2)
            {
                return;
            }

            string animalType = parts[0];
            string weight = parts[1];
            string key = $"{animalType}|{weight}";

            // TryGetValue tìm giá theo khóa và cho biết khóa đó có trong bảng giá hay không.
            if (!_currentPrices.TryGetValue(key, out string? price))
            {
                return;
            }

            ResetPriceButtons();
            button.Background = _selectedCellBackground;
            button.BorderBrush = _selectedBorder;

            _selectedAnimalType = animalType;
            _selectedWeight = weight;
            _selectedPrice = price;

            SelectedAnimalIcon.Source = LoadAnimalIcon(animalType);
            WeightText.Text = weight;
            PriceText.Text = price;
        }

        // Chọn file biểu tượng theo loại thú cưng và tải ảnh từ tài nguyên ứng dụng.
        private BitmapImage LoadAnimalIcon(string animalType)
        {
            string fileName = animalType switch
            {
                "Dog" => "DOG.png",
                "Cat" => "CAT.png",
                "Hamster" => "HAMSTER.png",
                _ => "DOG.png"
            };

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(
                $"pack://application:,,,/Animal-Spa;component/Asset/animal/{fileName}",
                UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        // Thiết lập bảng giá tương ứng với tên dịch vụ được chọn.
        private void ApplyPricingForService(string serviceName)
        {
            switch (serviceName)
            {
                case "Premium Bath":
                    SetPricing("180.000đ", "250.000đ", "150.000đ", "220.000đ", "100.000đ", "160.000đ");
                    break;

                case "Bath + Dry Combo":
                    SetPricing("150.000đ", "220.000đ", "130.000đ", "190.000đ", "85.000đ", "140.000đ");
                    break;

                case "Basic Trim":
                    SetPricing("140.000đ", "200.000đ", "120.000đ", "180.000đ", "80.000đ", "130.000đ");
                    break;

                case "Style Trim":
                    SetPricing("180.000đ", "250.000đ", "160.000đ", "220.000đ", "100.000đ", "150.000đ");
                    break;

                case "Full Groom":
                    SetPricing("250.000đ", "350.000đ", "220.000đ", "320.000đ", "140.000đ", "210.000đ");
                    break;

                case "Basic Ear Clean":
                    SetPricing("60.000đ", "80.000đ", "55.000đ", "75.000đ", "40.000đ", "55.000đ");
                    break;

                case "Deep Ear Clean":
                    SetPricing("90.000đ", "120.000đ", "80.000đ", "110.000đ", "55.000đ", "75.000đ");
                    break;

                case "Ear Care":
                    SetPricing("110.000đ", "140.000đ", "100.000đ", "130.000đ", "65.000đ", "90.000đ");
                    break;

                case "Basic Nail Trim":
                    SetPricing("70.000đ", "90.000đ", "60.000đ", "80.000đ", "40.000đ", "55.000đ");
                    break;

                case "Nail Grinding":
                    SetPricing("90.000đ", "120.000đ", "80.000đ", "105.000đ", "50.000đ", "70.000đ");
                    break;

                case "Paw Care":
                    SetPricing("120.000đ", "160.000đ", "100.000đ", "140.000đ", "65.000đ", "90.000đ");
                    break;

                default:
                    SetDefaultPricing();
                    break;
            }
        }

        // Áp dụng bảng giá mặc định khi khởi tạo hoặc khi tên dịch vụ chưa có bảng giá riêng.
        private void SetDefaultPricing()
        {
            SetPricing("120.000đ", "180.000đ", "100.000đ", "150.000đ", "70.000đ", "120.000đ");
        }

        // Hiển thị sáu mức giá theo loại thú cưng, nhóm cân nặng và cập nhật Dictionary tra cứu.
        private void SetPricing(
            string dogUnder,
            string dogOver,
            string catUnder,
            string catOver,
            string hamsterUnder,
            string hamsterOver)
        {
            DogUnderPrice.Text = dogUnder;
            DogOverPrice.Text = dogOver;
            CatUnderPrice.Text = catUnder;
            CatOverPrice.Text = catOver;
            HamsterUnderPrice.Text = hamsterUnder;
            HamsterOverPrice.Text = hamsterOver;

            _currentPrices.Clear();
            _currentPrices["Dog|Under 10kg"] = dogUnder;
            _currentPrices["Dog|Over 10kg"] = dogOver;
            _currentPrices["Cat|Under 10kg"] = catUnder;
            _currentPrices["Cat|Over 10kg"] = catOver;
            _currentPrices["Hamster|Under 10kg"] = hamsterUnder;
            _currentPrices["Hamster|Over 10kg"] = hamsterOver;
        }

        // Khôi phục biểu tượng mặc định, xóa dữ liệu đã chọn và bỏ đánh dấu các ô giá.
        private void ResetSelection()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(
                    "pack://application:,,,/Animal-Spa;component/Asset/ICON-PET.png",
                    UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                SelectedAnimalIcon.Source = bitmap;
            }
            catch
            {
                SelectedAnimalIcon.Source = null;
            }

            _selectedAnimalType = string.Empty;
            _selectedWeight = string.Empty;
            _selectedPrice = string.Empty;

            WeightText.Text = "Choose from table";
            PriceText.Text = "Choose from table";
            ResetPriceButtons();
        }

        // Khôi phục màu nền và viền ban đầu cho tất cả nút trong bảng giá.
        private void ResetPriceButtons()
        {
            Button[] buttons =
            {
                DogUnderButton,
                DogOverButton,
                CatUnderButton,
                CatOverButton,
                HamsterUnderButton,
                HamsterOverButton
            };

            foreach (Button button in buttons)
            {
                button.Background = _normalCellBackground;
                button.BorderBrush = Brushes.Transparent;
            }
        }

        // Phát sự kiện CloseRequested khi người dùng nhấn Cancel.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }

        // Kiểm tra lựa chọn và phát sự kiện ServiceChosen kèm thông tin dịch vụ khi nhấn Choose.
        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedAnimalType) ||
                string.IsNullOrWhiteSpace(_selectedWeight) ||
                string.IsNullOrWhiteSpace(_selectedPrice))
            {
                MessageBox.Show(
                    "Please choose an animal type and weight from the pricing matrix.",
                    "Animal Spa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Lấy các chữ số trong chuỗi giá để chuyển thành giá trị tiền dùng tính tổng.
            string digits = new string(_selectedPrice.Where(char.IsDigit).ToArray());
            decimal.TryParse(digits, out decimal priceValue);

            ServiceChosen?.Invoke(new BookingServiceItem
            {
                ServiceName = ServiceNameText.Text,
                Description = DescriptionText.Text,
                Duration = DurationText.Text,
                AnimalType = _selectedAnimalType,
                Weight = _selectedWeight,
                PriceText = _selectedPrice,
                PriceValue = priceValue,
                ServiceIcon = "/Animal-Spa;component/Asset/ICON-BATH.jpg"
            });
        }
    }
}
