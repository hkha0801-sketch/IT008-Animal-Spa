# Ba Dashboard – bản điều hướng theo vai trò

## Nguyên tắc

- Không tạo số liệu thống kê, lịch hẹn, tài khoản đăng nhập hoặc danh sách ca nhân viên giả **mới** cho Dashboard.
- Component có sẵn giữ nguyên nơi thực hiện nghiệp vụ; component chưa tồn tại được biểu diễn bằng vùng trống có mô tả rõ “Chưa có component”.
- Dashboard chuyển trang bằng menu; các component không còn xếp dài trên một trang.
- Dữ liệu mẫu **đã tồn tại trong các component gốc** vẫn là demo. Đây không phải dữ liệu đăng nhập thực.

## Receptionist Dashboard

| Menu | Nội dung |
| --- | --- |
| Tổng quan | Chỗ trống, chờ component tổng quan |
| Khách hàng | CustomerList → CustomerDetail / AddCustomer / AddPet |
| Thú cưng | Chỗ trống cho danh sách thú cưng, có hướng dẫn vào chi tiết khách hàng |
| Danh mục & dịch vụ | Category → Service → ServiceDetail → thêm vào Booking |
| Booking | Tab tạo booking + tab quản lý; khi tạo/nhận phân công, chuyển đến RoomStaffView |
| Phòng & nhân viên | Không mở RoomStaff đơn lẻ; dẫn về booking CONFIRMED để phân công |
| Báo cáo | Chỗ trống |

**Booking không bị thay đổi hướng xử lý:** `BookingView` tạo booking `PENDING`, sau đó `RoomStaffView` xác nhận, chọn phòng/nhân viên và lưu session; từ tab quản lý cũng có thể mở phân công. Các quy tắc nghiệp vụ và JSON vẫn nằm trong `BookingWorkflowStore.cs`.

## Customer Dashboard

- Tổng quan: chỗ trống.
- Hồ sơ & thú cưng: component `CustomerDetailView` và `AddPetView` hiện có, **dữ liệu mẫu ID = 3**, không ánh xạ tài khoản.
- Danh mục & dịch vụ: duyệt `CategoryView`, `ServiceView`, `ServiceDetailView`. Ẩn nút `Choose` trong chế độ Customer để tránh dẫn nhầm sang giỏ đặt lịch lễ tân.
- Lịch hẹn của tôi / Thanh toán: chỗ trống.

## Staff Dashboard

Có menu Tổng quan, Ca dịch vụ, Lịch làm việc, Hồ sơ cá nhân; các trang đều để trống với chú thích vì chưa có module Staff tương ứng. Không đưa màn hình đặt lịch/phân công của Receptionist vào Staff.

## Hướng dẫn chạy và kiểm thử

Trên Windows cài .NET 8 SDK (có hỗ trợ WPF):

```powershell
dotnet build Animal-Spa.sln
dotnet run --project Animal-Spa.csproj
```

Kiểm tra tĩnh độc lập với WPF:

```powershell
python Tests/DashboardStaticCheck.py
```

Thử thủ công:

1. Đăng nhập nhánh **Staff** với `admin / 123456` (tài khoản demo gốc) để vào Receptionist. Duyệt menu Khách hàng, vào chi tiết/thêm thú cưng, trở về danh sách, vào Danh mục & dịch vụ rồi chọn dịch vụ để đến Booking.
2. Tạo booking hợp lệ → Room & Staff hiện sau khi lưu → xác nhận booking, chọn phòng/nhân viên Available, lưu session → về tab quản lý booking.
3. Click menu Phòng & nhân viên: không được phân công nếu chưa chọn booking. Vào tab quản lý để chọn booking CONFIRMED.
4. Đăng nhập **User** → Customer: kiểm tra hồ sơ mẫu, danh mục/dịch vụ, không còn thao tác `Choose`; tab lịch hẹn/thanh toán chưa có chức năng.
5. Đăng nhập **Staff** bằng tài khoản demo khác admin → Staff: kiểm tra cả 4 mục đều là placeholder, không thấy thông tin booking của lễ tân.
6. Kiểm tra Đăng xuất ở 3 Dashboard.

> Chưa build/chạy GUI trên Windows trong môi trường tạo bản cập nhật. XAML đúng cú pháp XML không bảo đảm chắc chắn WPF chạy không lỗi.

## Các file sửa

- `Views/Receptionist/ReceptionistDashboard.xaml` và `.xaml.cs`
- `Views/Customer/CustomerDashboard.xaml` và `.xaml.cs`
- `Views/Staff/StaffDashboard.xaml` và `.xaml.cs`
- `Components/Receptionist/BookingView.Workflow.cs` (public methods để Dashboard chuyển tab)
- `Components/Receptionist/ServiceDetailView.xaml` và `.xaml.cs` (tắt `Choose` trong chế độ Customer)
- `Tests/DashboardStaticCheck.py` (mới)
