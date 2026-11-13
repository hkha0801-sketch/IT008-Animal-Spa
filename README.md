# Animal Spa

## Run on Windows

Xem [hướng dẫn chạy WPF client và kiểm tra kết nối](HUONG-DAN-CHAY.md).

Designer stability fix:
- Dashboard constructors skip runtime-only setup in Visual Studio Designer.
- Image loading in ServiceDetailView is guarded and cached safely.
- AddCustomer/AddPet skip runtime image initialization at design time.

## Receptionist workflow (demo mới)

Đã bổ sung tab quản lý booking ngay trong `BookingView` và dùng lại `RoomStaffView` gốc cho bước chọn phòng/nhân viên sau khi tạo booking. Hai XAML thừa đã được gỡ, vẫn giữ tìm/lọc/xem chi tiết/xác nhận/hủy và kiểm tra trùng lịch trước khi tạo session. Workflow chạy bằng JSON offline; **chưa tích hợp API server**. Xem [hướng dẫn và test](docs/BOOKING_WORKFLOW.md).

## Three role-based Dashboards (updated)

- Receptionist: menu quản lý khách hàng, dịch vụ và booking; `RoomStaffView` chỉ mở trong luồng booking.
- Customer: hồ sơ mẫu và duyệt dịch vụ; chưa có chức năng đặt lịch trực tuyến.
- Staff: menu có placeholder cho các component chưa được phát triển.
- Ba Dashboard đều có điều hướng và nút Đăng xuất. Không tạo số liệu thống kê mới giả lập.

Xem [docs/DASHBOARDS.md](docs/DASHBOARDS.md) và chạy kiểm tra tĩnh `python Tests/DashboardStaticCheck.py`.
