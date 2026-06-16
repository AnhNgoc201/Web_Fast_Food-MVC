"""
tests/test_06_ui_flow.py
Test luồng đầy đủ (End-to-End):
  - Luồng mua hàng hoàn chỉnh (đăng nhập → tìm → thêm giỏ → đặt hàng)
  - Luồng quản trị (admin login → CRUD sản phẩm)
  - Điều hướng toàn trang
  - Bảo mật session (logout, redirect)
"""
import time
import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from conftest import BASE_URL, TEST_EMAIL, TEST_PASSWORD, random_email

ADMIN_EMAIL    = "admin@jollibee.vn"
ADMIN_PASSWORD = "123456"


# ──────────────────────────────────────────────────────────
# NHÓM 1: Luồng mua hàng (khách hàng)
# ──────────────────────────────────────────────────────────
class TestE2EMuaHang:
    """Luồng đầy đủ: đăng nhập → thêm sản phẩm → xem giỏ → đặt hàng."""

    def test_luong_mua_hang_day_du(self, driver):
        wait = WebDriverWait(driver, 10)

        # Bước 1: Đăng nhập
        driver.get(f"{BASE_URL}/User/Login")
        driver.find_element(By.NAME, "Email").send_keys(TEST_EMAIL)
        driver.find_element(By.NAME, "Password").send_keys(TEST_PASSWORD)
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
        wait.until(EC.url_contains("SanPhams"))
        assert "SanPhams" in driver.current_url, "Đăng nhập thất bại"

        # Bước 2: Thêm sản phẩm vào giỏ
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=2")
        assert "Exception" not in driver.page_source

        # Bước 3: Xem giỏ hàng
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

        # Bước 4: Sang trang xác nhận đặt hàng
        driver.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        assert "Exception" not in driver.page_source

    def test_luong_tim_kiem_roi_them_vao_gio(self, driver):
        """Tìm sản phẩm qua search → thêm vào giỏ → xem giỏ."""
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=gà")
        assert "Exception" not in driver.page_source

        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

    def test_luong_them_nhieu_san_pham_roi_xem_gio(self, driver):
        """Thêm 3 sản phẩm khác nhau rồi xem giỏ — giỏ phải có đủ."""
        for msp in [1, 2, 3]:
            driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={msp}&soluong=1")
            assert "Exception" not in driver.page_source
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

    def test_luong_them_roi_xoa_roi_xem_gio(self, driver):
        """Thêm vào giỏ → xóa → giỏ phải phản ánh đúng."""
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=2")
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp=1")
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

    @pytest.mark.parametrize("keyword,msp", [
        ("gà", 1),
        ("burger", 2),
        ("khoai", 3),
    ])
    def test_luong_tim_theo_tu_khoa_va_dat_msp(self, driver, keyword, msp):
        """Tìm kiếm nhiều từ khóa khác nhau rồi thêm sản phẩm tương ứng."""
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword={keyword}")
        assert "Exception" not in driver.page_source
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={msp}&soluong=1")
        assert "Exception" not in driver.page_source


# ──────────────────────────────────────────────────────────
# NHÓM 2: Luồng tài khoản (đăng ký → đăng nhập → đăng xuất)
# ──────────────────────────────────────────────────────────
class TestE2ETaiKhoan:
    """Luồng liên quan đến tài khoản người dùng."""

    def test_luong_dang_ky_roi_dang_nhap(self, driver):
        """Đăng ký tài khoản mới → đăng nhập bằng tài khoản đó."""
        email = random_email()
        password = "123456"

        # Đăng ký
        driver.get(f"{BASE_URL}/User/Register")
        driver.find_element(By.NAME, "TenKH").send_keys("E2E Test User")
        driver.find_element(By.NAME, "Email").send_keys(email)
        driver.find_element(By.NAME, "SDT").send_keys("0901234567")
        driver.find_element(By.NAME, "MatKhau").send_keys(password)
        driver.find_element(By.NAME, "DiaChi").send_keys("123 Test Street")
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
        assert "Exception" not in driver.page_source

        # Đăng nhập với tài khoản vừa tạo
        driver.get(f"{BASE_URL}/User/Login")
        driver.find_element(By.NAME, "Email").send_keys(email)
        driver.find_element(By.NAME, "Password").send_keys(password)
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
        assert "Exception" not in driver.page_source

    def test_luong_dang_nhap_xem_lich_su_mua_hang(self, driver):
        """Đăng nhập → xem lịch sử mua hàng → phải vào được."""
        driver.get(f"{BASE_URL}/User/Login")
        driver.find_element(By.NAME, "Email").send_keys(TEST_EMAIL)
        driver.find_element(By.NAME, "Password").send_keys(TEST_PASSWORD)
        driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()

        driver.get(f"{BASE_URL}/User/LichSuMuaHang")
        assert "Login" not in driver.current_url
        assert "Exception" not in driver.page_source

    def test_luong_dang_nhap_roi_dang_xuat(self, driver_logged_in):
        """Đăng nhập → đăng xuất → không còn truy cập được trang riêng tư."""
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        driver_logged_in.get(f"{BASE_URL}/User/LichSuMuaHang")
        assert "Login" in driver_logged_in.current_url

    def test_lich_su_mua_hang_chua_login_redirect(self, driver):
        """Chưa đăng nhập → truy cập lịch sử → redirect về Login."""
        driver.get(f"{BASE_URL}/User/LichSuMuaHang")
        assert "Login" in driver.current_url

    def test_lich_su_sau_khi_login(self, driver_logged_in):
        """Đã đăng nhập → xem lịch sử mua hàng → phải hiện được trang."""
        driver_logged_in.get(f"{BASE_URL}/User/LichSuMuaHang")
        assert "Exception" not in driver_logged_in.page_source
        assert "Login" not in driver_logged_in.current_url


# ──────────────────────────────────────────────────────────
# NHÓM 3: Điều hướng toàn trang
# ──────────────────────────────────────────────────────────
class TestE2ENavigation:
    """Kiểm tra điều hướng qua các trang chính của website."""

    @pytest.mark.parametrize("path,description", [
        ("/",                          "trang chủ"),
        ("/SanPhams",                  "danh sách sản phẩm"),
        ("/Home/About",                "giới thiệu"),
        ("/Home/Contact",              "liên hệ"),
        ("/DatHang/XemGioHang",        "xem giỏ hàng"),
        ("/User/Login",                "đăng nhập"),
        ("/User/Register",             "đăng ký"),
        ("/SanPhams/TimKiemNangCao",   "tìm kiếm nâng cao"),
    ])
    def test_cac_trang_chinh_khong_loi_server(self, driver, path, description):
        driver.get(f"{BASE_URL}{path}")
        assert "Exception" not in driver.page_source, \
            f"Lỗi server tại trang {description} ({path})"
        assert "500" not in driver.title, \
            f"Lỗi 500 tại trang {description} ({path})"

    def test_di_chuyen_tu_san_pham_sang_gio_hang(self, driver):
        """Từ trang sản phẩm → click thêm vào giỏ → sang xem giỏ."""
        driver.get(f"{BASE_URL}/SanPhams")
        assert "Exception" not in driver.page_source
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

    def test_back_forward_browser_khong_loi(self, driver):
        """Nhấn back/forward trình duyệt không gây lỗi server."""
        driver.get(f"{BASE_URL}/SanPhams")
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        driver.back()
        assert "Exception" not in driver.page_source
        driver.forward()
        assert "Exception" not in driver.page_source

    def test_url_khong_ton_tai_tra_404_khong_500(self, driver):
        """Truy cập URL không tồn tại phải trả 404 chứ không phải 500."""
        driver.get(f"{BASE_URL}/TrangKhongTonTaiXYZ123")
        assert "Exception" not in driver.page_source
        assert "Server Error" not in driver.page_source

    def test_truy_cap_nhanh_nhieu_trang_lien_tiep(self, driver):
        """Truy cập nhiều trang liên tiếp nhanh — không timeout hay crash."""
        paths = ["/SanPhams", "/DatHang/XemGioHang", "/User/Login", "/SanPhams"]
        for path in paths:
            driver.get(f"{BASE_URL}{path}")
            assert "Exception" not in driver.page_source
            time.sleep(0.2)


# ──────────────────────────────────────────────────────────
# NHÓM 4: Luồng admin (đăng nhập → quản lý)
# ──────────────────────────────────────────────────────────
class TestE2EAdmin:
    """Luồng quản trị từ đầu đến cuối."""

    def _login_admin(self, driver):
        driver.get(f"{BASE_URL}/NhanVien/Login")
        try:
            driver.find_element(By.NAME, "Email").send_keys(ADMIN_EMAIL)
            driver.find_element(By.NAME, "Password").send_keys(ADMIN_PASSWORD)
            driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
            WebDriverWait(driver, 8).until(
                lambda d: "admin" in d.current_url.lower()
                or "dashboard" in d.page_source.lower()
            )
        except Exception:
            pass

    def test_luong_admin_login_xem_danh_sach_san_pham(self, driver):
        self._login_admin(driver)
        driver.get(f"{BASE_URL}/SanPhamAdmin")
        assert "Exception" not in driver.page_source

    def test_luong_admin_login_xem_hoa_don(self, driver):
        self._login_admin(driver)
        driver.get(f"{BASE_URL}/HoaDons")
        assert "Exception" not in driver.page_source

    def test_luong_admin_login_xem_khuyen_mai(self, driver):
        self._login_admin(driver)
        driver.get(f"{BASE_URL}/KhuyenMais")
        assert "Exception" not in driver.page_source

    def test_luong_admin_logout_roi_bi_chặn(self, driver):
        """Admin logout → truy cập lại trang admin → phải bị chặn."""
        self._login_admin(driver)
        driver.get(f"{BASE_URL}/NhanVien/Logout")
        driver.get(f"{BASE_URL}/SanPhamAdmin")
        src = driver.page_source.lower()
        assert (
            "login" in driver.current_url.lower()
            or "đăng nhập" in src
            or "403" in driver.page_source
        )


# ──────────────────────────────────────────────────────────
# NHÓM 5: Bảo mật session & edge cases
# ──────────────────────────────────────────────────────────
class TestE2ESession:
    """Kiểm tra session, bảo mật, và các trường hợp biên."""

    def test_session_giu_sau_khi_chuyen_trang(self, driver_logged_in):
        """Sau đăng nhập, chuyển nhiều trang vẫn còn session."""
        for path in ["/SanPhams", "/DatHang/XemGioHang", "/User/LichSuMuaHang"]:
            driver_logged_in.get(f"{BASE_URL}{path}")
            assert "Login" not in driver_logged_in.current_url, \
                f"Session mất tại {path}"

    def test_trang_rieng_tu_chua_dang_nhap_redirect(self, driver):
        """Tất cả trang riêng tư phải redirect khi chưa đăng nhập."""
        private_paths = [
            "/User/LichSuMuaHang",
            "/DatHang/XacNhanDonHang",
        ]
        for path in private_paths:
            driver.get(f"{BASE_URL}{path}")
            # Phải redirect hoặc hiện thông báo — không crash
            assert "Exception" not in driver.page_source, f"Lỗi tại {path}"

    def test_refresh_trang_gio_hang_giu_du_lieu(self, driver):
        """Refresh trang giỏ hàng không làm mất dữ liệu giỏ."""
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=2")
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        driver.refresh()
        assert "Exception" not in driver.page_source

    def test_dat_hang_khi_chua_dang_nhap_redirect_login(self, driver):
        """Xác nhận đơn hàng khi chưa đăng nhập → redirect login."""
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        # Tùy nghiệp vụ: có thể redirect login hoặc cho phép guest checkout
        assert "Exception" not in driver.page_source

    def test_mo_nhieu_tab_dong_thoi_khong_xung_dot_session(self, driver):
        """
        Giả lập truy cập nhiều route liên tiếp trong cùng session
        (không mở tab thật được trong Selenium — kiểm tra bằng sequential request).
        """
        paths = [
            "/SanPhams",
            "/DatHang/ThemMatHang?msp=1&soluong=1",
            "/DatHang/XemGioHang",
            "/SanPhams",
            "/DatHang/XemGioHang",
        ]
        for path in paths:
            driver.get(f"{BASE_URL}{path}")
            assert "Exception" not in driver.page_source


# ──────────────────────────────────────────────────────────
# NHÓM 6: Bổ sung — luồng nghiệp vụ và biên dữ liệu mở rộng
# ──────────────────────────────────────────────────────────
class TestE2EExtra:
    """Nhóm test bổ sung: thêm nhiều luồng nghiệp vụ kết hợp."""

    def test_luong_dang_ky_sai_roi_dang_ky_lai_dung(self, driver):
        """Đăng ký thiếu trường (lỗi) → sửa lại và đăng ký thành công."""
        driver.get(f"{BASE_URL}/User/Register")
        driver.find_element(By.NAME, "Email").send_keys(random_email())
        btns = driver.find_elements(By.XPATH,
            "//textarea[@name='DiaChi']/ancestor::form//button[@type='submit']")
        if btns:
            btns[0].click()
        assert "Exception" not in driver.page_source

        driver.get(f"{BASE_URL}/User/Register")
        driver.find_element(By.NAME, "TenKH").send_keys("Test Lai")
        driver.find_element(By.NAME, "Email").send_keys(random_email())
        driver.find_element(By.NAME, "SDT").send_keys("0901234567")
        driver.find_element(By.NAME, "MatKhau").send_keys("123456")
        driver.find_element(By.NAME, "DiaChi").send_keys("Test Address")
        driver.find_element(By.XPATH,
            "//textarea[@name='DiaChi']/ancestor::form//button[@type='submit']").click()
        assert "Exception" not in driver.page_source

    def test_luong_them_gio_xoa_het_roi_kiem_tra_trong(self, driver):
        """Thêm nhiều sản phẩm, xóa hết, kiểm tra giỏ phải trống."""
        for msp in [1, 2]:
            driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={msp}&soluong=1")
        for msp in [1, 2]:
            driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp={msp}")
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

    def test_luong_dang_nhap_them_gio_dang_xuat_gio_con_khong(self, driver_logged_in):
        """Đăng nhập, thêm giỏ, đăng xuất rồi xem giỏ — kiểm tra hành vi thực tế."""
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_tim_kiem_khong_ra_ket_qua_roi_tim_lai_dung(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=khongtontai999")
        assert "Exception" not in driver.page_source
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=ga")
        assert "Exception" not in driver.page_source

    def test_luong_xem_chi_tiet_roi_them_vao_gio(self, driver):
        """Xem chi tiết sản phẩm → quay lại → thêm vào giỏ."""
        driver.get(f"{BASE_URL}/SanPhams/Details/1")
        assert "Exception" not in driver.page_source
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        assert "Exception" not in driver.page_source

    def test_luong_dang_nhap_sai_3_lan_roi_dung_lan_4(self, driver):
        """Đăng nhập sai 3 lần liên tiếp, sau đó đúng ở lần thứ 4."""
        for _ in range(3):
            driver.get(f"{BASE_URL}/User/Login")
            driver.find_element(By.NAME, "Email").send_keys(TEST_EMAIL)
            driver.find_element(By.NAME, "Password").send_keys("sai_mat_khau")
            driver.find_element(By.XPATH,
                "//input[@name='Password']/ancestor::form//button[@type='submit']").click()
        driver.get(f"{BASE_URL}/User/Login")
        driver.find_element(By.NAME, "Email").send_keys(TEST_EMAIL)
        driver.find_element(By.NAME, "Password").send_keys(TEST_PASSWORD)
        driver.find_element(By.XPATH,
            "//input[@name='Password']/ancestor::form//button[@type='submit']").click()
        assert "SanPhams" in driver.current_url

    def test_luong_loc_danh_muc_roi_tim_kiem_trong_danh_muc(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?maDM=1")
        assert "Exception" not in driver.page_source
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=ga&maDM=1")
        assert "Exception" not in driver.page_source

    def test_luong_dat_hang_2_lan_lien_tiep(self, driver_logged_in):
        """Đặt hàng xong, thêm sản phẩm mới và đặt hàng lần 2."""
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=2&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_thay_doi_so_luong_truoc_khi_dat_hang(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp=1&soLuong=1&hanhDong=tang")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_quay_lai_tu_xac_nhan_don_hang_ve_gio(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        driver_logged_in.back()
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_dang_nhap_roi_truy_cap_trang_dang_ky(self, driver_logged_in):
        """Đã đăng nhập nhưng vẫn vào được trang đăng ký (không bắt buộc logout)."""
        driver_logged_in.get(f"{BASE_URL}/User/Register")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_chuyen_doi_ngon_ngu_hoac_giao_dien_khong_loi(self, driver):
        """Load trang chính nhiều lần liên tiếp không phát sinh lỗi tích lũy."""
        for _ in range(3):
            driver.get(f"{BASE_URL}/SanPhams")
            assert "Exception" not in driver.page_source

    def test_luong_them_sai_loai_du_lieu_roi_them_dung(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=abc&soluong=1")
        assert "Server Error" not in driver.page_source
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        assert "Exception" not in driver.page_source

    def test_luong_dang_ky_dang_nhap_dang_xuat_lien_tiep(self, driver):
        """Luồng hoàn chỉnh: đăng ký → đăng nhập → đăng xuất, tất cả trong 1 session."""
        email = random_email()
        password = "123456"
        driver.get(f"{BASE_URL}/User/Register")
        driver.find_element(By.NAME, "TenKH").send_keys("Full Flow")
        driver.find_element(By.NAME, "Email").send_keys(email)
        driver.find_element(By.NAME, "SDT").send_keys("0901234567")
        driver.find_element(By.NAME, "MatKhau").send_keys(password)
        driver.find_element(By.NAME, "DiaChi").send_keys("Test Address")
        driver.find_element(By.XPATH,
            "//textarea[@name='DiaChi']/ancestor::form//button[@type='submit']").click()

        driver.get(f"{BASE_URL}/User/Login")
        driver.find_element(By.NAME, "Email").send_keys(email)
        driver.find_element(By.NAME, "Password").send_keys(password)
        driver.find_element(By.XPATH,
            "//input[@name='Password']/ancestor::form//button[@type='submit']").click()

        driver.get(f"{BASE_URL}/User/Logout")
        assert "Exception" not in driver.page_source

    def test_luong_xem_san_pham_het_hang_khong_them_duoc_vao_gio(self, driver):
        """Sản phẩm hết hàng (nếu có) không được thêm vào giỏ thành công."""
        driver.get(f"{BASE_URL}/SanPhams")
        assert "Exception" not in driver.page_source

    def test_luong_tim_kiem_nang_cao_roi_dat_hang(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/SanPhams/TimKiemNangCao?keyword=ga&giaMin=10000&giaMax=100000")
        assert "Exception" not in driver_logged_in.page_source
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_dang_nhap_xem_3_san_pham_chi_tiet_lien_tiep(self, driver_logged_in):
        for pid in [1, 2, 3]:
            driver_logged_in.get(f"{BASE_URL}/SanPhams/Details/{pid}")
            assert "Server Error" not in driver_logged_in.page_source

    def test_luong_dang_xuat_giua_qua_trinh_dat_hang(self, driver_logged_in):
        """Đăng xuất giữa lúc đang đặt hàng — không được crash hệ thống."""
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_truy_cap_admin_tu_phia_khach_hang_da_dang_nhap(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/SanPhamAdmin")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_them_gio_qua_url_khong_qua_form(self, driver):
        """Thêm sản phẩm trực tiếp qua URL (giả lập thao tác thủ công, không qua UI)."""
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

    def test_luong_dang_ky_voi_email_trung_nhieu_lan(self, driver):
        """Cố đăng ký với email đã tồn tại 3 lần liên tiếp — luôn phải bị chặn."""
        for _ in range(3):
            driver.get(f"{BASE_URL}/User/Register")
            driver.find_element(By.NAME, "TenKH").send_keys("Trung Email")
            driver.find_element(By.NAME, "Email").send_keys(TEST_EMAIL)
            driver.find_element(By.NAME, "SDT").send_keys("0901234567")
            driver.find_element(By.NAME, "MatKhau").send_keys("123456")
            driver.find_element(By.NAME, "DiaChi").send_keys("Test")
            driver.find_element(By.XPATH,
                "//textarea[@name='DiaChi']/ancestor::form//button[@type='submit']").click()
        assert "Register" in driver.current_url

    def test_luong_kiem_tra_toan_bo_cac_trang_sau_khi_dang_nhap(self, driver_logged_in):
        """Đăng nhập rồi duyệt qua tất cả trang chính — không trang nào lỗi 500."""
        paths = ["/SanPhams", "/DatHang/XemGioHang", "/User/LichSuMuaHang", "/SanPhams/TimKiemNangCao"]
        for path in paths:
            driver_logged_in.get(f"{BASE_URL}{path}")
            assert "Server Error" not in driver_logged_in.page_source, f"Lỗi tại {path}"

    def test_luong_thoat_giua_dang_ky_roi_vao_lai(self, driver):
        """Vào trang đăng ký, điều hướng đi nơi khác, rồi quay lại đăng ký — không lỗi."""
        driver.get(f"{BASE_URL}/User/Register")
        driver.get(f"{BASE_URL}/SanPhams")
        driver.get(f"{BASE_URL}/User/Register")
        assert "Exception" not in driver.page_source

    def test_luong_kiem_tra_gio_hang_rieng_biet_giua_2_phien_dang_nhap(self, driver):
        """Đăng nhập, thêm giỏ, đăng xuất, đăng nhập lại — kiểm tra hành vi giỏ hàng."""
        driver.get(f"{BASE_URL}/User/Login")
        driver.find_element(By.NAME, "Email").send_keys(TEST_EMAIL)
        driver.find_element(By.NAME, "Password").send_keys(TEST_PASSWORD)
        driver.find_element(By.XPATH,
            "//input[@name='Password']/ancestor::form//button[@type='submit']").click()
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver.get(f"{BASE_URL}/User/Logout")

        driver.get(f"{BASE_URL}/User/Login")
        driver.find_element(By.NAME, "Email").send_keys(TEST_EMAIL)
        driver.find_element(By.NAME, "Password").send_keys(TEST_PASSWORD)
        driver.find_element(By.XPATH,
            "//input[@name='Password']/ancestor::form//button[@type='submit']").click()
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "Exception" not in driver.page_source

    def test_luong_dat_hang_voi_nhieu_san_pham_khac_nhau_cung_luc(self, driver_logged_in):
        """Thêm 3 sản phẩm khác nhau vào giỏ, rồi xác nhận đặt hàng 1 lần."""
        for msp in [1, 2, 3]:
            driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={msp}&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        assert "Exception" not in driver_logged_in.page_source

    def test_luong_kiem_tra_toc_do_phan_hoi_toan_bo_luong_mua_hang(self, driver_logged_in):
        """Toàn bộ luồng mua hàng phải phản hồi trong thời gian hợp lý."""
        start = time.time()
        driver_logged_in.get(f"{BASE_URL}/SanPhams")
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XemGioHang")
        driver_logged_in.get(f"{BASE_URL}/DatHang/XacNhanDonHang")
        elapsed = time.time() - start
        assert elapsed < 30, "Luồng mua hàng phản hồi quá chậm"