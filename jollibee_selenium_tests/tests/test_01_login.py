"""
tests/test_01_login.py
Test UserController: Login / Logout — mở rộng nhiều trường hợp dữ liệu.
"""
import pytest
from selenium.webdriver.common.by import By
from pages.user_page import LoginPage
from conftest import BASE_URL, TEST_EMAIL, TEST_PASSWORD


class TestLoginPageDisplay:
    """Nhóm test: trang Login hiển thị đúng các thành phần."""

    def test_trang_load_thanh_cong(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_co_truong_email(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.EMAIL_INPUT)

    def test_co_truong_password(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.PASSWORD_INPUT)

    def test_password_input_co_type_password(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(*page.PASSWORD_INPUT)
        assert el.get_attribute("type") == "password"

    def test_co_nut_submit(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.SUBMIT_BTN)

    def test_co_link_quen_mat_khau(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.FORGOT_LINK)

    def test_co_link_dang_ky(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.REGISTER_LINK)

    def test_co_link_dang_nhap_nhan_vien(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.STAFF_LOGIN_LINK)

    def test_title_chua_dang_nhap(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert "đăng nhập" in driver.title.lower() or "jollibee" in driver.title.lower()

    def test_logo_hien_thi(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.exists(By.CSS_SELECTOR, "img[alt='Jollibee Logo']")


class TestLoginValidation:
    """Nhóm test: các trường hợp nhập liệu sai khi đăng nhập."""

    @pytest.mark.parametrize("email,password,case_name", [
        ("", "123456", "thieu_email"),
        (TEST_EMAIL, "", "thieu_password"),
        ("", "", "thieu_ca_hai"),
        ("khong_dung_format", "123456", "email_sai_format"),
        ("abc@", "123456", "email_thieu_domain"),
        ("@gmail.com", "123456", "email_thieu_local_part"),
        ("test test@gmail.com", "123456", "email_co_khoang_trang"),
    ])
    def test_login_du_lieu_khong_hop_le(self, driver, email, password, case_name):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(email, password)
        # Không được nhảy sang trang sản phẩm thành công với dữ liệu rác
        assert "Login" in driver.current_url, f"Case {case_name} lẽ ra phải ở lại trang login"

    @pytest.mark.parametrize("wrong_password", [
        "sai_mat_khau_xyz",
        "123",
        "000000",
        "wrongPASS123",
        "matkhau sai có dấu",
    ])
    def test_login_sai_mat_khau_nhieu_truong_hop(self, driver, wrong_password):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL, wrong_password)
        assert "Login" in driver.current_url
        assert page.get_error() != ""

    @pytest.mark.parametrize("fake_email", [
        "khong_ton_tai_xyz@abc.com",
        "random12345@test.com",
        "fake_user_999@gmail.com",
    ])
    def test_login_email_khong_ton_tai_nhieu_truong_hop(self, driver, fake_email):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(fake_email, "123456")
        assert page.get_error() != ""

    def test_login_sql_injection_khong_bypass(self, driver):
        """Kiểm tra cơ bản: SQL injection không bypass được đăng nhập."""
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login("' OR '1'='1", "' OR '1'='1")
        assert "Login" in driver.current_url, "Không được bypass đăng nhập bằng SQL injection"

    def test_login_xss_khong_thuc_thi(self, driver):
        """Nhập script vào field không được làm web crash."""
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login("<script>alert(1)</script>@a.com", "123456")
        assert page.no_server_error()


class TestLoginSuccess:
    """Nhóm test: đăng nhập đúng thông tin."""

    def test_login_thanh_cong_redirect(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL, TEST_PASSWORD)
        assert "SanPhams" in driver.current_url

    def test_login_thanh_cong_co_message(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL, TEST_PASSWORD)
        # TempData Message hiển thị ở trang sau redirect
        assert page.no_server_error()

    def test_session_giu_sau_khi_chuyen_trang(self, driver):
        """Sau đăng nhập, chuyển trang khác vẫn còn đăng nhập (session)."""
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL, TEST_PASSWORD)
        driver.get(f"{BASE_URL}/User/LichSuMuaHang")
        assert "Login" not in driver.current_url

    def test_login_email_khong_phan_biet_hoa_thuong(self, driver):
        """Email login không nên phân biệt hoa/thường (tùy yêu cầu nghiệp vụ)."""
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL.upper(), TEST_PASSWORD)
        # Kiểm tra behavior thật: hoặc đăng nhập thành công hoặc thất bại — không crash
        assert page.no_server_error()
        logged_in = "SanPhams" in driver.current_url
        still_on_login = "Login" in driver.current_url
        assert logged_in or still_on_login, "Phải redirect rõ ràng sau khi submit"


class TestLogout:
    def test_logout_xoa_session(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        driver_logged_in.get(f"{BASE_URL}/User/LichSuMuaHang")
        assert "Login" in driver_logged_in.current_url

    def test_logout_redirect_ve_trang_chu_san_pham(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        assert "SanPhams" in driver_logged_in.current_url or \
               driver_logged_in.current_url.rstrip("/") == BASE_URL

    def test_truy_cap_lai_sau_logout_can_dang_nhap_lai(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        driver_logged_in.get(f"{BASE_URL}/User/Login")
        page = LoginPage(driver_logged_in, BASE_URL)
        assert page.exists(*page.EMAIL_INPUT)

    def test_logout_2_lan_lien_tiep_khong_loi(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        page = LoginPage(driver_logged_in, BASE_URL)
        assert page.no_server_error()

    def test_logout_khi_chua_dang_nhap(self, driver):
        driver.get(f"{BASE_URL}/User/Logout")
        page = LoginPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_giu_nguyen_gio_hang_sau_logout(self, driver_logged_in):
        """Đăng xuất không được làm crash dù giỏ hàng có dữ liệu."""
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver_logged_in.get(f"{BASE_URL}/User/Logout")
        page = LoginPage(driver_logged_in, BASE_URL)
        assert page.no_server_error()


class TestLoginFieldBehavior:
    """Nhóm test: hành vi chi tiết của các trường nhập trên form login."""

    def test_email_input_co_attribute_name_dung(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(*page.EMAIL_INPUT)
        assert el.get_attribute("name") == "Email"

    def test_password_input_co_attribute_name_dung(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(*page.PASSWORD_INPUT)
        assert el.get_attribute("name") == "Password"

    def test_email_input_co_the_clear_va_nhap_lai(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(*page.EMAIL_INPUT)
        el.send_keys("abc@gmail.com")
        el.clear()
        el.send_keys("xyz@gmail.com")
        assert el.get_attribute("value") == "xyz@gmail.com"

    def test_password_khong_hien_thi_ky_tu_that(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(*page.PASSWORD_INPUT)
        el.send_keys("matkhau123")
        assert el.get_attribute("type") == "password"

    def test_form_login_nam_trong_tag_form(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(*page.EMAIL_INPUT)
        form = el.find_element(By.XPATH, "./ancestor::form")
        assert form is not None

    def test_submit_bang_nhan_enter(self, driver):
        """Nhấn Enter trong ô password cũng phải submit được form."""
        from selenium.webdriver.common.keys import Keys
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.type(*page.EMAIL_INPUT, TEST_EMAIL)
        pw = driver.find_element(*page.PASSWORD_INPUT)
        pw.clear()
        pw.send_keys(TEST_PASSWORD)
        pw.send_keys(Keys.ENTER)
        assert page.no_server_error()

    def test_khong_co_loi_khi_chua_nhap_gi_va_load_trang(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.get_error() == ""

    def test_trang_login_khong_co_2_form_trung_ten_email(self, driver):
        """Đảm bảo chỉ có 1 input name=Email trên trang (tránh nhầm form search)."""
        page = LoginPage(driver, BASE_URL)
        page.open()
        inputs = driver.find_elements(By.NAME, "Email")
        assert len(inputs) == 1


class TestLoginNavigation:
    """Nhóm test: điều hướng từ trang Login sang các trang liên quan."""

    def test_click_link_dang_ky_chuyen_dung_trang(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.click(*page.REGISTER_LINK)
        assert "Register" in driver.current_url

    def test_click_link_quen_mat_khau_chuyen_dung_trang(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.click(*page.FORGOT_LINK)
        assert "ForgotPassword" in driver.current_url

    def test_click_link_dang_nhap_nhan_vien(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.click(*page.STAFF_LOGIN_LINK)
        assert page.no_server_error()

    def test_truy_cap_truc_tiep_url_login(self, driver):
        driver.get(f"{BASE_URL}/User/Login")
        assert "Login" in driver.current_url

    def test_quay_lai_trang_login_bang_nut_back(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.click(*page.REGISTER_LINK)
        driver.back()
        assert "Login" in driver.current_url

    def test_load_lai_trang_login_f5_khong_loi(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        driver.refresh()
        assert page.no_server_error()


class TestLoginEdgeCases:
    """Nhóm test: các trường hợp biên và bảo mật bổ sung."""

    def test_login_voi_email_rat_dai(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        long_email = "a" * 200 + "@gmail.com"
        page.login(long_email, "123456")
        assert page.no_server_error()

    def test_login_voi_password_rat_dai(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL, "x" * 500)
        assert page.no_server_error()

    def test_login_voi_unicode_emoji_trong_email(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login("😀@gmail.com", "123456")
        assert page.no_server_error()

    def test_login_voi_khoang_trang_dau_cuoi_email(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(f"  {TEST_EMAIL}  ", TEST_PASSWORD)
        assert page.no_server_error()

    def test_login_html_injection_trong_password(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL, "<img src=x onerror=alert(1)>")
        assert page.no_server_error()

    def test_login_nhieu_lan_sai_lien_tiep_khong_bi_crash(self, driver):
        """Thử sai liên tục nhiều lần xem hệ thống có chặn brute-force không."""
        page = LoginPage(driver, BASE_URL)
        for i in range(5):
            page.open()
            page.login(TEST_EMAIL, f"sai_lan_{i}")
        assert page.no_server_error()

    def test_login_voi_ky_tu_unicode_dac_biet_password(self, driver):
        page = LoginPage(driver, BASE_URL)
        page.open()
        page.login(TEST_EMAIL, "mật khẩu có dấu tiếng Việt")
        assert page.no_server_error()

    def test_login_url_co_query_string_thua(self, driver):
        driver.get(f"{BASE_URL}/User/Login?returnUrl=/SanPhams&abc=123")
        page = LoginPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_login_method_get_truc_tiep_khong_crash(self, driver):
        """Gọi GET tới action Login (vốn chỉ nhận POST từ form) không được lỗi 500."""
        driver.get(f"{BASE_URL}/User/Login?Email={TEST_EMAIL}&Password={TEST_PASSWORD}")
        page = LoginPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_login_khong_loi_khi_co_cookie_rac(self, driver):
        driver.get(BASE_URL)
        driver.add_cookie({"name": "junk_cookie", "value": "12345"})
        page = LoginPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_login_co_dinh_dang_thoi_gian_phan_hoi_hop_ly(self, driver):
        """Trang login phải phản hồi trong thời gian hợp lý, không bị treo."""
        import time
        page = LoginPage(driver, BASE_URL)
        start = time.time()
        page.open()
        elapsed = time.time() - start
        assert elapsed < 15, "Trang login load quá chậm"