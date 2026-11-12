# Animal Spa

## Run on Windows

Xem [hướng dẫn chạy WPF client và kiểm tra kết nối](HUONG-DAN-CHAY.md).

Designer stability fix:
- Dashboard constructors skip runtime-only setup in Visual Studio Designer.
- Image loading in ServiceDetailView is guarded and cached safely.
- AddCustomer/AddPet skip runtime image initialization at design time.
