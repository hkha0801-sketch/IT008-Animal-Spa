# Chạy WPF client Animal Spa

Cần Windows và .NET 8 SDK, hoặc Visual Studio với workload **.NET desktop development** và hỗ trợ .NET 8.

1. Chạy `pet-spa-management` trên cùng máy theo [hướng dẫn server](../pet-spa-management/HUONG-DAN-CHAY.md). Kiểm tra `http://localhost:5052/health` trả về HTTP `200`.
2. Mở PowerShell tại thư mục `Animal-Spa` rồi chạy lệnh sau. Nếu dùng Visual Studio, mở `Animal-Spa.sln` và nhấn **F5**.

   ```powershell
   dotnet run --project Animal-Spa.csproj
   ```

3. Màn hình đăng nhập hiện **Server connected** khi API phản hồi thành công. Nếu hiện **Server unavailable**, rê chuột lên thông báo để xem chi tiết. Khi server đã chạy ở cổng `5052`, nhấn **Retry connection** để kiểm tra lại.

Trạng thái kết nối chỉ xác nhận API có phản hồi. Nút **Login** tiếp tục dùng luồng đăng nhập cục bộ hiện có.
