using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Animal_Spa.Components.Receptionist
{
    public partial class CategoryView : UserControl
    {
        // Các Brush quy định màu nền và viền của thẻ danh mục theo trạng thái chọn.
        private readonly Brush _normalBackground =
            new SolidColorBrush(Color.FromRgb(169, 203, 233));

        private readonly Brush _selectedBackground =
            new SolidColorBrush(Color.FromRgb(67, 139, 221));

        private readonly Brush _selectedBorder =
            new SolidColorBrush(Color.FromRgb(57, 126, 214));

        // Phát tên danh mục được chọn, Action<string> nhận phương thức void có một tham số string.
        public event Action<string>? CategorySelected;

        // Khởi tạo giao diện và đưa các thẻ danh mục về trạng thái ban đầu.
        public CategoryView()
        {
            InitializeComponent();
            ResetCategoryCards();
        }

        // Ẩn nút thêm danh mục, hiện danh mục chăm sóc móng và đặt lại trạng thái chọn.
        public void SetCustomerMode()
        {
            AddCategoryButton.Visibility = Visibility.Collapsed;
            NailCategoryButton.Visibility = Visibility.Visible;
            ResetCategoryCards();
        }

        // Lấy tên danh mục từ Tag, tô màu thẻ được nhấn và phát sự kiện CategorySelected.
        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string category)
            {
                return;
            }

            ResetCategoryCards();
            button.Background = _selectedBackground;
            button.BorderBrush = _selectedBorder;
            CategorySelected?.Invoke(category);
        }

        // Khôi phục màu nền và viền ban đầu cho tất cả thẻ danh mục.
        private void ResetCategoryCards()
        {
            BathCategoryButton.Background = _normalBackground;
            HairCategoryButton.Background = _normalBackground;
            EarCategoryButton.Background = _normalBackground;
            NailCategoryButton.Background = _normalBackground;

            BathCategoryButton.BorderBrush = Brushes.Transparent;
            HairCategoryButton.BorderBrush = Brushes.Transparent;
            EarCategoryButton.BorderBrush = Brushes.Transparent;
            NailCategoryButton.BorderBrush = Brushes.Transparent;
        }
    }
}
