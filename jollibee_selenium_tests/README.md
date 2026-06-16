# Selenium Test — Web Jollibee MVC (Web_Do_An_Nhanh)

Bộ test tự động Selenium cho website bán đồ ăn nhanh Jollibee, viết bằng
**ASP.NET MVC (C#)**. Test được viết bằng Python + pytest + Selenium WebDriver,
theo mô hình **Page Object Model (POM)**, và tự xuất báo cáo kết quả ra file Excel.

**Tổng số: 270 test case** (đã tính cả các trường hợp dữ liệu lặp qua `parametrize`).

---

## Cấu trúc project

```
jollibee_selenium_tests/
├── conftest.py              ← Cấu hình chung: URL, tài khoản test, khởi tạo driver
├── conftest_reporter.py     ← Plugin tự xuất báo cáo kết quả ra Excel (.xlsx)
├── requirements.txt         ← Danh sách thư viện cần cài
├── pages/                   ← Page Object Model — mỗi trang 1 class
│   ├── base_page.py         ← Class cha: các hàm dùng chung (open, click, type...)
│   ├── user_page.py         ← Đăng nhập, Đăng ký, Quên mật khẩu
│   ├── sanpham_page.py      ← Trang sản phẩm, tìm kiếm, tìm kiếm nâng cao
│   └── giohang_page.py      ← Giỏ hàng, xác nhận đặt hàng
├── tests/                   ← Toàn bộ test case, chia theo chức năng
│   ├── test_01_login.py     ← Đăng nhập / Đăng xuất             (50 case)
│   ├── test_02_register.py  ← Đăng ký tài khoản                 (50 case)
│   ├── test_03_sanpham.py   ← Sản phẩm, tìm kiếm, danh mục       (50 case)
│   ├── test_04_giohang.py   ← Giỏ hàng, đặt hàng, mã giảm giá    (50 case)
│   ├── test_05_admin.py     ← Phân quyền trang quản trị          (50 case)
│   └── test_06_ui_flow.py   ← Luồng người dùng đầy đủ (E2E)      (50 case)
└── test_reports/            ← Báo cáo Excel tự sinh sau khi chạy test
    ├── test_results_PASS.xlsx
    └── test_results_FAIL.xlsx
```

---

## Cài đặt

### 1. Cài Python (nếu chưa có)
Tải tại https://python.org/downloads — nhớ tick **"Add python.exe to PATH"**.

### 2. Cài thư viện

```bash
pip install -r requirements.txt
pip install openpyxl
```

### 3. Có sẵn Chrome
Selenium sẽ tự điều khiển Chrome đã cài trên máy — không cần tải ChromeDriver
riêng nếu dùng Selenium ≥ 4.6 (tự quản lý driver).

---

## Cấu hình trước khi chạy

Mở `conftest.py`, kiểm tra/sửa các giá trị:

```python
BASE_URL      = "https://localhost:44325"      # ← Port web ASP.NET (xem ở Visual Studio khi F5)
TEST_EMAIL    = "anhngoc2xx5@gmail.com"         # ← Tài khoản khách hàng có sẵn trong DB
TEST_PASSWORD = "123456"
TEST_HOTEN    = "Phạm Ánh Ngọc"
TEST_SDT      = "0393901164"
TEST_DIACHI   = "Tây Ninh"
```

> **Bắt buộc**: tài khoản `TEST_EMAIL` / `TEST_PASSWORD` phải tồn tại sẵn
> trong database trước khi chạy test, vì nhiều test case (giỏ hàng, đặt hàng,
> lịch sử mua hàng...) cần đăng nhập để hoạt động.

Web ASP.NET phải đang **chạy** (nhấn F5 trong Visual Studio) trước khi chạy Selenium.

---

## Chạy test

```bash
# Chạy toàn bộ 270 test case
python -m pytest tests/ -v

# Chạy riêng 1 file (ví dụ chỉ test đăng nhập)
python -m pytest tests/test_01_login.py -v

# Chạy 1 class cụ thể trong file
python -m pytest tests/test_01_login.py::TestLoginSuccess -v

# Chạy ngầm, không hiện cửa sổ Chrome (headless)
# → bỏ comment dòng "# options.add_argument('--headless')" trong conftest.py

# Sinh thêm báo cáo HTML (ngoài Excel đã tự sinh)
python -m pytest tests/ -v --html=bao_cao.html --self-contained-html
```

Sau khi chạy xong, vào thư mục `test_reports/` để xem 2 file Excel:
- `test_results_PASS.xlsx` — toàn bộ case PASS, kèm bảng tổng hợp (sheet Summary)
- `test_results_FAIL.xlsx` — toàn bộ case FAIL, kèm thông tin lỗi chi tiết

Mỗi dòng trong Excel gồm: mã test case, kịch bản, tiền điều kiện, các bước thực
hiện, dữ liệu test, kết quả mong đợi, kết quả thực tế, và trạng thái PASS/FAIL —
đúng format báo cáo kiểm thử chuẩn.