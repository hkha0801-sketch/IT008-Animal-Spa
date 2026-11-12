using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Animal_Spa.Components.Receptionist
{
    public partial class RoomStaffView : UserControl
    {
        // Các Brush quy định màu viền và chấm tròn của lựa chọn phòng, nhân viên.
        private readonly Brush _selectedBorder = new SolidColorBrush(Color.FromRgb(62, 132, 216));
        private readonly Brush _normalBorder = Brushes.Transparent;
        private readonly Brush _selectedDot = new SolidColorBrush(Color.FromRgb(168, 201, 232));

        // Phát thông báo hủy chọn, Action nhận phương thức void không tham số.
        public event Action? CancelRequested;
        // Phát thông báo xác nhận phòng và nhân viên đang được chọn.
        public event Action? Confirmed;

        // Lưu tên phòng và nhân viên đang được chọn.
        public string SelectedRoom { get; private set; } = "ROOM 1";
        public string SelectedStaff { get; private set; } = "Nguyen Van B";

        // Khởi tạo các thành phần giao diện chọn phòng và nhân viên.
        public RoomStaffView()
        {
            InitializeComponent();
        }

        // Đặt lựa chọn mặc định về phòng 1 và nhân viên Nguyen Van B.
        public void PrepareSelection()
        {
            SelectRoom(Room1Button, Room1Outer, Room1Inner, "ROOM 1");
            SelectStaff(StaffBButton, StaffBOuter, StaffBInner, "Nguyen Van B");
        }

        // Lấy tên phòng từ Tag của nút được nhấn và cập nhật lựa chọn tương ứng.
        private void Room_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string room)
            {
                return;
            }

            if (button == Room1Button)
            {
                SelectRoom(Room1Button, Room1Outer, Room1Inner, room);
            }
            else if (button == Room2Button)
            {
                SelectRoom(Room2Button, Room2Outer, Room2Inner, room);
            }
            else if (button == Room3Button)
            {
                SelectRoom(Room3Button, Room3Outer, Room3Inner, room);
            }
        }

        // Lấy tên nhân viên từ Tag của nút được nhấn và cập nhật lựa chọn tương ứng.
        private void Staff_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string staff)
            {
                return;
            }

            if (button == StaffAButton)
            {
                SelectStaff(StaffAButton, StaffAOuter, StaffAInner, staff);
            }
            else if (button == StaffBButton)
            {
                SelectStaff(StaffBButton, StaffBOuter, StaffBInner, staff);
            }
            else if (button == StaffCButton)
            {
                SelectStaff(StaffCButton, StaffCOuter, StaffCInner, staff);
            }
        }

        // Bỏ đánh dấu phòng cũ, tô viền và chấm tròn phòng mới rồi hiển thị số phòng.
        private void SelectRoom(Button selectedButton, Ellipse selectedOuter, Ellipse selectedInner, string room)
        {
            ResetRooms();
            selectedButton.BorderBrush = _selectedBorder;
            selectedOuter.Stroke = _selectedBorder;
            selectedInner.Fill = _selectedDot;
            SelectedRoom = room;
            SelectedRoomText.Text = room.Replace("ROOM ", string.Empty);
        }

        // Bỏ đánh dấu nhân viên cũ, tô viền và chấm tròn lựa chọn mới rồi hiển thị tên.
        private void SelectStaff(Button selectedButton, Ellipse selectedOuter, Ellipse selectedInner, string staff)
        {
            ResetStaff();
            selectedButton.BorderBrush = _selectedBorder;
            selectedOuter.Stroke = _selectedBorder;
            selectedInner.Fill = _selectedDot;
            SelectedStaff = staff;
            SelectedStaffText.Text = staff;
        }

        // Đưa tất cả lựa chọn phòng về trạng thái chưa được đánh dấu.
        private void ResetRooms()
        {
            ResetChoice(Room1Button, Room1Outer, Room1Inner);
            ResetChoice(Room2Button, Room2Outer, Room2Inner);
            ResetChoice(Room3Button, Room3Outer, Room3Inner);
        }

        // Đưa tất cả lựa chọn nhân viên về trạng thái chưa được đánh dấu.
        private void ResetStaff()
        {
            ResetChoice(StaffAButton, StaffAOuter, StaffAInner);
            ResetChoice(StaffBButton, StaffBOuter, StaffBInner);
            ResetChoice(StaffCButton, StaffCOuter, StaffCInner);
        }

        // Khôi phục viền nút, viền vòng tròn ngoài và màu vòng tròn trong của một lựa chọn.
        private void ResetChoice(Button button, Ellipse outer, Ellipse inner)
        {
            button.BorderBrush = _normalBorder;
            outer.Stroke = Brushes.Black;
            inner.Fill = Brushes.Transparent;
        }

        // Phát sự kiện CancelRequested khi người dùng nhấn Cancel.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke();
        }

        // Phát sự kiện Confirmed khi người dùng xác nhận lựa chọn hiện tại.
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Confirmed?.Invoke();
        }
    }
}
