### video gameplay
https://drive.google.com/drive/folders/1n_ut427G4Umsg74nOpl4L1oztloFr1Ec?usp=drive_link

Hệ thống Nông trại:

Trồng trọt các loại cây (Cà rốt, Bông cải xanh, v.v.) và chăn nuôi (Bò).

Quản lý vòng đời đầy đủ: Trồng (Growing) -> Sẵn sàng (Ready) -> Hỏng (Spoiled).

Cơ chế "Hỏng": Nếu không thu hoạch sản phẩm cuối cùng sau 1 giờ, vật phẩm và cây trồng sẽ bị hỏng và phải dọn dẹp.

Hệ thống Kinh tế:

Mua/Bán vật phẩm với giá được cấu hình từ file config.

Hỗ trợ Bán sỉ (Wholesale): Logic cửa hàng tự động kiểm tra và làm tròn số lượng mua tối thiểu (ví dụ: CauliFlower).

Hệ thống Công nhân (Worker AI):

Công nhân tự động tìm và di chuyển đến các lô đất đã sẵn sàng để thu hoạch.

Mỗi hành động (ví dụ: thu hoạch) tốn một khoảng thời gian (cấu hình trong CSV, ví dụ: 2 phút).

Quản lý trạng thái đầy đủ (Idle, MovingToTarget, Working, MovingHome).

Nâng cấp & Mở rộng:

Thuê thêm công nhân.

Mở khóa thêm các lô đất mới.

Nâng cấp trang thiết bị để tăng tốc độ sản xuất (ví dụ: 10% mỗi cấp).

Tiến trình Offline (Offline Progress):

Trò chơi tự động tính toán tất cả tiến trình (cây trồng lớn lên, bị hỏng) trong thời gian người chơi offline.

Hệ thống ValidateGameStateOnLoadUseCase sẽ tự động xử lý các tác vụ của công nhân đã hoàn thành trong lúc offline.

Điều kiện thắng:

Trò chơi hiển thị thông báo chiến thắng khi người chơi đạt mốc 1.000.000 vàng (được định nghĩa trong config).

🏗️ Kiến trúc Kỹ thuật: Clean Architecture
Điểm cốt lõi của dự án là việc áp dụng kiến trúc Clean Architecture (hoặc Onion Architecture) để đảm bảo logic game hoàn toàn độc lập với Unity Engine, giúp việc bảo trì, mở rộng và kiểm thử (testing) trở nên cực kỳ dễ dàng.

Dự án được chia thành 4 lớp (Assemblies), được định nghĩa bởi các file .asmdef:

1. 🔵 Domain (Lõi)
Thư mục: Myfarm/Domain

Trách nhiệm: Chứa các quy tắc nghiệp vụ cốt lõi và mô hình dữ liệu (Models). Đây là trái tim của trò chơi.

Ví dụ: FarmPlot.cs, Player.cs, ProductionConfig.cs.

Quy tắc: KHÔNG chứa using UnityEngine;. Lớp này không biết Unity là gì và có thể chạy trên bất kỳ nền tảng C# nào (ví dụ: Console, Server).

2. 🟢 Application (Ứng dụng)
Thư mục: Myfarm/Application

Trách nhiệm: Điều phối các hành động (Use Cases) mà người dùng có thể thực hiện. Nó định nghĩa các Interfaces (hợp đồng) mà các lớp bên ngoài phải tuân theo.

Ví dụ: BuyItemUseCase.cs, HarvestUseCase.cs, IGameDataRepository.cs, IConfigLoader.cs.

Quy tắc: Chỉ phụ thuộc vào Domain.

3. 🟡 Infrastructure (Cơ sở hạ tầng)
Thư mục: Myfarm/Infrastructures

Trách nhiệm: Cung cấp các công cụ kỹ thuật cụ thể để thực thi các "hợp đồng" (Interfaces) từ tầng Application.

Ví dụ:

CsvConfigLoader.cs: Đọc tất cả dữ liệu game (giá cả, thời gian) từ file CSV.

JsonDataRepository.cs: Lưu/Tải game bằng file JSON.

UnityWorldTimeService.cs: Cung cấp thời gian thực của hệ thống.

Quy tắc: Phụ thuộc vào Application và Domain.

4. 🔴 Presentation (Giao diện)
Thư mục: Myfarm/Presentation

Trách nhiệm: Là lớp Unity Engine. Chứa tất cả MonoBehaviour, UI, Prefabs, và Models 3D.

Ví dụ: FarmPlotView.cs, UIManager.cs, GameManager.cs (Composition Root).

Quy tắc: Phụ thuộc vào cả 3 lớp còn lại. Đây là điểm khởi đầu (Entry Point) của game, nơi các Use Case được gọi khi người dùng nhấn nút.

📊 Thiết kế Hướng dữ liệu (Data-Driven)
Toàn bộ các chỉ số cân bằng game (giá bán, giá mua, thời gian mọc, vòng đời,...) đều được lưu trữ trong các file CSV (nằm trong Resources/Configs).

Điều này cho phép Game Designer dễ dàng cân bằng game chỉ bằng cách chỉnh sửa file Excel/CSV mà không cần can thiệp vào code. Hệ thống CsvConfigLoader sẽ tự động đọc các thay đổi này khi khởi động game.

🧪 Kiểm thử (Unit Testing)
Dự án bao gồm một bộ Unit Test đầy đủ cho 2 lớp quan trọng nhất là Domain và Application (sử dụng NUnit và NSubstitute).

Thư mục: Tests/

Các bài test bao gồm:

FarmPlotTests.cs: Kiểm tra logic vòng đời, tính toán offline, và cơ chế hỏng (spoil).

BuyItemUseCaseTests.cs: Kiểm tra logic mua lẻ và mua sỉ.

UpdateGameTickUseCaseTests.cs: Kiểm tra AI của worker và các trường hợp edge case (ví dụ: plot bị hỏng khi worker đang làm).

ValidateGameStateOnLoadUseCaseTests.cs: Kiểm tra logic xử lý offline.

Việc này đảm bảo logic cốt lõi của game luôn chạy đúng, độc lập với bất kỳ thay đổi nào về giao diện (UI) ở tầng Presentation.



📅 Quá trình Thực hiện Dự án (Development Process)
Dự án được triển khai trong vòng 7 ngày với các giai đoạn cụ thể như sau:

Giai đoạn 1: Phân tích & Thiết kế (Ngày 1 - 2)

Phân tích kỹ lưỡng yêu cầu đề bài (Gameplay, Economy, AI Worker).

Xác định kiến trúc phần mềm phù hợp: Quyết định sử dụng Clean Architecture kết hợp Domain-Driven Design (DDD) để tách biệt logic game khỏi Unity Engine.

Giai đoạn 2: Xây dựng Core Logic (Ngày 3)

Triển khai tầng Domain: Định nghĩa các Entities (Farm, Worker, Plot) và Value Objects.

Triển khai tầng Application: Viết các Use Cases chính (Harvest, BuyItem, HireWorker).

Xây dựng tầng Infrastructure: Cài đặt các repositories lưu trữ dữ liệu (Json) và hệ thống load config (CSV).

Giai đoạn 3: Kiểm thử Đơn vị (Ngày 4)

Viết Unit Tests để kiểm chứng độ chính xác của tầng Domain và Application.

Đảm bảo các logic quan trọng (tính tiền, thời gian mọc cây, xử lý offline) hoạt động đúng trước khi gắn vào Unity.

Giai đoạn 4: Tích hợp UI & Presentation (Ngày 5)

Xây dựng tầng Presentation trong Unity: Tạo các Prefabs, UI Panels.

Kết nối View (FarmPlotView, UIManager) với Use Cases thông qua GameManager.

Cài đặt hệ thống Sự kiện (EventNotifier) để cập nhật giao diện tự động.

Giai đoạn 5: Hoàn thiện & Tối ưu (Ngày 6 - 7)

Polish Game: Tinh chỉnh trải nghiệm người dùng, thêm hiệu ứng hình ảnh (LeanTween) cho animal và phản hồi UI.

Refactor Code: Rà soát và làm sạch code, xử lý các trường hợp ngoại lệ (Edge Cases).

Documentation: Viết tài liệu hướng dẫn (README), bổ sung comment code và chuẩn bị build demo.


Hạn chế & Nâng cấp Tương lai
Dự án có nền tảng Clean Architecture vững chắc, nhưng có thể được cải thiện cho quy mô sản phẩm thực tế:

Hiệu năng (Performance):

Hạn chế: Sử dụng Instantiate/Destroy nhiều cho UI và đối tượng game, có thể gây giật lag khi game mở rộng.

Nâng cấp: Tối ưu hóa bằng Object Pooling (tái sử dụng đối tượng) và UI Virtualization (cho các danh sách dài).

Quản lý Tài nguyên (Asset Management):

Hạn chế: Đang dùng API Resources.Load cũ để tải Prefabs và Configs.

Nâng cấp: Chuyển sang hệ thống Addressables để tải tài nguyên bất đồng bộ (giảm lag, giảm thời gian tải game) và hỗ trợ cập nhật nội dung (DLC) mà không cần build lại game.

Dữ liệu (Data):

Hạn chế: File save .json (text) dễ bị người dùng chỉnh sửa (hack vàng).

Nâng cấp: Mã hóa (Encrypt) file save hoặc chuyển sang dùng cơ sở dữ liệu local như SQLite để tăng cường bảo mật và hiệu năng truy vấn.

Kiểm thử (Testing):

Hạn chế: Đã có Unit Test cho tầng Domain/Application, nhưng chưa có test tự động cho tầng Presentation (UI).

Nâng cấp: Bổ sung Integration Tests (Play Mode) để xác thực logic của UI (ví dụ: nhấn nút Nâng cấp có thực sự cập nhật Text Vàng không).
