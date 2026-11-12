using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Animal_Spa.Components.Receptionist
{
    public partial class AddPetView : UserControl
    {
        // Phát thông báo khi hủy thêm thú cưng, Action nhận phương thức void không tham số.
        public event Action? CancelRequested;

        // Lưu mã khách hàng sở hữu thú cưng đang được thêm.
        public int? CurrentCustomerId { get; private set; }
        // Lưu đường dẫn ảnh thú cưng mà người dùng đã chọn.
        private string? _selectedPhotoPath;

        // Khởi tạo giao diện và tải ảnh mặc định khi chạy ứng dụng.
        public AddPetView()
        {
            InitializeComponent();

            // Bỏ qua việc tải ảnh khi giao diện đang mở trong trình thiết kế.
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            LoadDefaultUploadFrame();
        }

        // Chuẩn bị form trống và giữ mã khách hàng được truyền vào.
        public void PrepareForCustomer(int customerId)
        {
            CurrentCustomerId = customerId;
            ClearForm(keepCustomer: true);
        }

        // Tải ảnh mặc định cho vùng chọn ảnh thú cưng.
        private void LoadDefaultUploadFrame()
        {
            try
            {
                var uri = new Uri(
                    "pack://application:,,,/Animal-Spa;component/Asset/FRAME-UPLOAD.jpg",
                    UriKind.Absolute);

                // BitmapImage đại diện cho hình ảnh lấy từ URI tài nguyên của ứng dụng.
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                SetUploadImage(bitmap);
            }
            catch
            {
                UploadRectangle.Fill = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                UploadFallback.Visibility = Visibility.Visible;
            }
        }

        // Khi người dùng nhấp vào vùng ảnh, mở cửa sổ chọn ảnh từ máy tính.
        private void UploadPhotoArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // OpenFileDialog cho phép duyệt và chọn một file ảnh có sẵn.
            var dialog = new OpenFileDialog
            {
                Title = "Choose pet photo",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var bitmap = LoadBitmapWithoutLockingFile(dialog.FileName);
                SetUploadImage(bitmap);

                _selectedPhotoPath = dialog.FileName;
                StatusText.Text = "Photo selected. Click the photo again to replace it.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Could not load image: {ex.Message}";
            }
        }

        // Đọc toàn bộ ảnh vào bộ nhớ và đóng file sau khi đọc xong.
        private static BitmapImage LoadBitmapWithoutLockingFile(string path)
        {
            using var stream = File.OpenRead(path);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        // Hiển thị ảnh lên UploadRectangle bằng ImageBrush và ẩn nội dung thay thế.
        private void SetUploadImage(ImageSource imageSource)
        {
            UploadRectangle.Fill = new ImageBrush(imageSource)
            {
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };

            UploadFallback.Visibility = Visibility.Collapsed;
        }

        // Hiển thị trạng thái form và mã khách hàng nếu có khi nhấn Save.
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = CurrentCustomerId is int customerId
                ? $"UI ready for customer #{customerId} — Save is not connected to pet creation yet."
                : "UI ready — Save is not connected to pet creation yet.";
        }

        // Xóa form, bỏ mã khách hàng và phát sự kiện CancelRequested khi nhấn Cancel.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClearForm(keepCustomer: false);
            CancelRequested?.Invoke();
        }

        // Xóa thông tin đã nhập và khôi phục ảnh mặc định, keepCustomer quyết định giữ mã khách hàng.
        public void ClearForm(bool keepCustomer = false)
        {
            PetNameTextBox.Clear();
            SpeciesTextBox.Clear();
            MaleRadioButton.IsChecked = false;
            FemaleRadioButton.IsChecked = false;
            SpecialTextBox.Clear();
            BirthDayTextBox.Clear();
            StatusText.Text = string.Empty;
            _selectedPhotoPath = null;

            if (!keepCustomer)
                CurrentCustomerId = null;

            UploadRectangle.Fill = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            UploadFallback.Visibility = Visibility.Visible;
            LoadDefaultUploadFrame();
        }
    }
}
