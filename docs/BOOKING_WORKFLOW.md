# Animal Spa — BookingView → RoomStaffView (WPF)

## 1. Cấu trúc sau khi chỉnh

Không tạo thêm hai file `BookingManagementView.xaml` và `BookingScheduleDialog.xaml`.

- `Components/Receptionist/BookingView.xaml` + `.xaml.cs`: giữ giao diện chọn khách hàng/dịch vụ, thêm ngày giờ hẹn/tên thú cưng/ghi chú trực tiếp và thêm **tab Danh sách / quản lý** để tìm kiếm, lọc, xem chi tiết, xác nhận/hủy.
- `Components/Receptionist/BookingView.Workflow.cs`: xử lý tab quản lý, tìm kiếm không dấu, trạng thái và kiểm tra thao tác hợp lệ.
- `Components/Receptionist/RoomStaffView.xaml` + `.xaml.cs`: tái sử dụng **component Room & Staff gốc** như ảnh mẫu. Chỉ hiển thị phòng/nhân viên trống cho khung giờ booking, đánh dấu Busy màu đỏ, disable lựa chọn sai, yêu cầu xác nhận PENDING trước khi tạo service session.
- `Views/Receptionist/ReceptionistDashboard.xaml` + `.xaml.cs`: điều hướng BookingView → RoomStaffView → BookingView. Hai component không hiển thị đồng thời.
- `Models/BookingWorkflowModels.cs` và `Services/BookingWorkflowStore.cs`: workflow demo lưu JSON, kiểm tra trạng thái và kiểm tra xung đột lúc ghi.

## 2. Luồng sử dụng

1. Mở Receptionist Dashboard, chọn khách hàng và ít nhất một dịch vụ.
2. Trong tab **Tạo booking**, nhập tên thú cưng, ngày, giờ HH:mm và ghi chú (không bắt buộc); nhấn **Tiếp tục: ROOM & STAFF**.
3. Ứng dụng lưu booking `PENDING`, ẩn BookingView và mở **RoomStaffView**.
4. Bấm **Xác nhận booking**, nhận trạng thái `CONFIRMED`; lúc này các phòng/nhân viên khả dụng mới cho phép click. Nhân viên / phòng bận hiển thị `Busy` và bị khóa.
5. Chọn 1 phòng và 1 nhân viên đang trống. Nút **Tạo service session** chỉ sáng khi cả hai hợp lệ. Bấm để tạo session `ASSIGNED`. Quay lại tab quản lý booking để thấy mã service session và phân công.
6. Bấm **Quay lại** ở RoomStaff để trở về BookingView tab quản lý; booking được lưu trước đó không bị xóa. Trong tab quản lý có thể tìm `BK-0102`, tên khách, thú cưng; lọc, xem chi tiết; Confirm PENDING / Cancel PENDING hoặc CONFIRMED chưa có session; hoặc bấm **Chọn phòng & nhân viên** trên booking CONFIRMED để quay lại RoomStaff.

## 3. Quy tắc nghiệp vụ

- Confirm: chỉ `PENDING`.
- Cancel: chỉ `PENDING` / `CONFIRMED` và chưa có session hoạt động.
- Session: chỉ từ `CONFIRMED` chưa có session; tài nguyên active, không trùng giờ; hệ thống kiểm tra lại khi lưu.
- Collision: `[start,end)` trùng khi `aStart < bEnd && bStart < aEnd`; các ca chạm mốc đầu/cuối không bị tính trùng.
- `RoomStaffView` hiện **3 phòng** và **3 nhân viên** tương ứng với dữ liệu demo trong repository. Nếu backend thêm tài nguyên khác, UI phải thay các card tĩnh thành ItemsControl/DataTemplate.

## 4. Chạy và test

Yêu cầu Windows, Visual Studio với .NET desktop development hoặc .NET 8 SDK:

```powershell
dotnet run --project Animal-Spa.csproj
dotnet run --project Tests/WorkflowSmoke/WorkflowSmoke.csproj
```

Dữ liệu JSON offline mặc định: `%LOCALAPPDATA%\AnimalSpa\receptionist-workflow.json`. Mở lần đầu sẽ có seed booking, phòng, nhân viên và ca mẫu. Muốn reset: **thoát ứng dụng**, sao lưu rồi xóa file JSON này, sau đó chạy lại.

## 5. Hạn chế

- Chưa có backend cho booking list/available rooms/available staff/service sessions; đây là demo JSON cục bộ, **không có đồng bộ qua thiết bị hoặc DB thực**.
- Kiểm tra `lock` chỉ bảo vệ một tiến trình desktop; backend thật phải kiểm tra xung đột + insert service session trong transaction / cơ chế chống concurrent double-booking.
- Chưa kiểm thử build WPF trên Windows trong môi trường tạo ZIP. Cần chạy hai lệnh ở trên.
- Tên thú cưng nhập thủ công và ID khách hàng từ danh sách mẫu, chưa thay bằng petId / danh sách phòng, nhân viên từ backend.
