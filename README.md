# Carrot-Framework-Unity

Tạo template và plugin giúp làm game, ứng dụng Unity nhanh hơn với hệ thống giao diện tự thiết kế, các module dùng lại được và nhiều chức năng hỗ trợ đa nền tảng.

<p align="center">
  <img src="./carrot_img/carrot_28.png" alt="Carrot Framework Logo" width="160">
</p>

## Các chức năng chính

Các chức năng được xây dựng để các game và ứng dụng trong hệ thống Carrot có thể thiết lập, cập nhật và tái sử dụng dễ dàng trên nhiều nền tảng mà không cần can thiệp sâu vào từng hệ điều hành.

### Store App
- **Rate**: Hiển thị cửa sổ đánh giá ứng dụng và mở trang store theo nền tảng.
- **Share**: Chia sẻ ứng dụng bằng link mặc định hoặc link tùy chỉnh.
- **App Other**: Gợi ý thêm ứng dụng/game khác trong hệ sinh thái Carrot.
- **Store Support**: Hỗ trợ Google Play, Samsung Galaxy Store, Microsoft Store, Amazon Appstore, Carrot Store, Huawei Store, Itch và Uptodown.

### UI và UX
- **Box List**: Tạo danh sách đối tượng dạng list.
- **Box Grid**: Tạo danh sách đối tượng dạng lưới.
- **Setting Box**: Tạo màn hình cài đặt gồm âm thanh, rung, nhạc nền, theme, ngôn ngữ, rate, share, remove ads, restore, full screen và reset dữ liệu.
- **Message Window**: Hiển thị thông báo, xác nhận Yes/No và các trạng thái biểu tượng.
- **Input Window**: Nhập dữ liệu dạng text hoặc slider, có callback sau khi hoàn tất.
- **Loading Window**: Hiển thị loading thường hoặc loading có thanh tiến trình.
- **Window Manager**: Quản lý stack cửa sổ, đóng cửa sổ cuối, đóng tất cả cửa sổ và xử lý phím Escape/back.
- **Dynamic Button**: Tạo button và item UI bằng prefab dùng chung.

### Function
- **Location**: Lấy vị trí hiện tại, hiển thị bản đồ và chuyển tọa độ thành tên địa điểm.
- **Log**: Ghi và xem lịch sử hoạt động của ứng dụng.
- **JSON**: Phân tích chuỗi JSON, đọc dữ liệu dạng dictionary/list và chuyển đối tượng thành JSON.
- **HTTP Request**: Gửi GET/POST, request ẩn, tải dữ liệu, tải ảnh và tải mp3.
- **Image Cache**: Tải ảnh từ URL, lưu vào PlayerPrefs hoặc file, load lại ảnh cho UI Image/SpriteRenderer.
- **File Tool**: Lưu, đọc, xóa, kiểm tra file trong thư mục thường và persistent data path.
- **App State**: Kiểm tra online/offline, kiểm tra mất internet, reset dữ liệu và chụp screenshot.
- **Utility**: Delay callback, random item, tạo ID, đổi âm click, rung thiết bị và đổi màu chủ đạo.

### User
- **Login**: Đăng nhập người dùng.
- **Register**: Đăng ký tài khoản mới.
- **Lost Password**: Hiển thị luồng quên mật khẩu.
- **Update Info**: Cập nhật thông tin tài khoản.
- **View Info**: Xem thông tin người dùng hiện tại hoặc theo ID.
- **Change Password**: Thay đổi mật khẩu.
- **Avatar**: Tải và hiển thị avatar, hỗ trợ danh sách avatar theo giới tính.
- **User Session**: Lưu, đọc và xóa dữ liệu đăng nhập cục bộ.
- **Advanced Fields**: Hỗ trợ các trường thông tin mở rộng cho hồ sơ người dùng.

### Language
- **Multi Language**: Chọn và lưu ngôn ngữ đang dùng.
- **Local Language File**: Nạp file ngôn ngữ từ Resources bằng `FileNameLangApp`.
- **Language UI Binding**: Tự cập nhật text/image trên các đối tượng `Carrot_lang_show`.
- **Language Lookup**: Lấy chuỗi theo key với giá trị mặc định qua `Carrot.L()` hoặc `lang.Val()`.
- **Language List**: Hiển thị danh sách ngôn ngữ, chọn ngôn ngữ và chạy callback sau khi chọn.

### CameraPro
- **Camera**: Chụp ảnh từ camera.
- **Photo Editor**: Trình chỉnh sửa ảnh cơ bản.
- **Photo List**: Xem danh sách ảnh đã tạo/lưu.
- **Photo Tool**: Cắt ảnh, resize/crop texture và chuyển `Texture2D` thành `Sprite`.

### Theme
- **Color Theme**: Chọn màu chủ đạo cho ứng dụng.
- **List Theme**: Bộ sưu tập chủ đề cho ứng dụng.
- **Custom Theme**: Tùy biến theme riêng.
- **List Color**: Danh sách màu sắc.
- **Mix Color**: Trộn màu và xử lý màu HSB.

### Game
- **GamePad**: Tạo tay cầm ảo theo ID.
- **D-pad / Console Control**: Điều hướng danh sách button bằng phím lên/xuống/enter và ScrollRect.
- **GamePad Detection**: Callback phát hiện trạng thái sử dụng gamepad/console.
- **Background Music**: Hiển thị danh sách nhạc nền, phát/tạm dừng, tải mp3 và lưu lựa chọn nhạc.
- **Reward Music Unlock**: Mở khóa nhạc nền qua quảng cáo thưởng hoặc in-app purchase.
- **Top Player**: Bảng xếp hạng người chơi.
- **Rank Type**: Thêm nhiều loại bảng xếp hạng với icon riêng.
- **Score Update**: Cập nhật điểm người chơi và sắp xếp bảng xếp hạng.

### Ads
- **View Ads**: Hiển thị quảng cáo AdMob, Carrot và Vungle.
- **Reward Advertising**: Xem quảng cáo để nhận phần thưởng.
- **Remove Ads**: Mua gói bỏ quảng cáo.
- **Ads Window**: Hiển thị cửa sổ quảng cáo dùng prefab riêng.

### In-App Purchase
- **Unity IAP**: Mua sản phẩm qua Unity Purchasing.
- **CarrotPay**: Hỗ trợ kiểu thanh toán CarrotPay.
- **Restore Purchase**: Khôi phục các sản phẩm đã mua.
- **Product Mapping**: Lấy product id theo index cấu hình.
- **Purchase Callback**: Xử lý thành công/thất bại khi mua hàng.

### Carrot Hub và dữ liệu online
- **Supabase Table**: Đọc bảng Supabase, có hoặc không có filter.
- **Firestore Document**: Lấy document theo query hoặc theo collection/document path.
- **Update Field**: Cập nhật một field trong document online.
- **Structured Query**: Tạo query có select, where, order và limit bằng `StructuredQuery`.
- **Song API**: Tìm kiếm, liệt kê và lấy bài hát theo ID.
- **Report API**: Gửi báo cáo lỗi/góp ý với email, loại báo cáo, message, user ID và object ID.
- **Remote File URL**: Tạo URL file từ host/worker cấu hình.

### Platform
- **Android / iOS / Windows / Web / macOS / Linux**: Cấu hình app theo nền tảng.
- **App / Game Mode**: Tùy chỉnh theo loại ứng dụng hoặc game.
- **Publish / Develop Mode**: Chuyển chế độ phát hành và phát triển.
- **Full Screen**: Bật/tắt full screen cho nền tảng hỗ trợ.
