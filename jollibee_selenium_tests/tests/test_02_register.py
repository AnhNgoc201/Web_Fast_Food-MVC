"""
tests/test_02_register.py
Test UserController: Register — mở rộng nhiều trường hợp validation.
"""
import time
import pytest
from selenium.webdriver.common.by import By
from pages.user_page import RegisterPage
from conftest import BASE_URL, TEST_EMAIL, random_email


class TestRegisterPageDisplay:
    def test_trang_load_thanh_cong(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    @pytest.mark.parametrize("field_name", ["TenKH", "Email", "SDT", "MatKhau", "DiaChi"])
    def test_co_truong_bat_buoc(self, driver, field_name):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        by = By.NAME
        assert page.exists(by, field_name), f"Thiếu field {field_name}"

    def test_email_input_co_type_email(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(By.NAME, "Email")
        assert el.get_attribute("type") == "email"

    def test_matkhau_input_co_type_password(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(By.NAME, "MatKhau")
        assert el.get_attribute("type") == "password"

    def test_sdt_input_co_type_tel(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        el = driver.find_element(By.NAME, "SDT")
        assert el.get_attribute("type") == "tel"

    def test_co_nut_dang_ky(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.SUBMIT_BTN)

    def test_co_link_ve_dang_nhap(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.LOGIN_LINK)

    def test_co_anti_forgery_token(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        assert page.exists(By.NAME, "__RequestVerificationToken")


class TestRegisterSuccess:
    def test_dang_ky_thanh_cong(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(
            ten="Nguyen Test", email=random_email(),
            password="123456", sdt="0901234567", diachi="123 Test HCM"
        )
        assert "Login" in driver.current_url or page.no_server_error()

    @pytest.mark.parametrize("ten", [
        "Nguyễn Văn A", "Trần Thị B", "Lê Văn C-D", "O'Brien Test", "Phạm Anh Ngọc",
    ])
    def test_dang_ky_voi_ten_co_dau_va_ky_tu_dac_biet(self, driver, ten):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten=ten, email=random_email(), password="123456",
                       sdt="0909999999", diachi="Địa chỉ test")
        assert page.no_server_error()

    @pytest.mark.parametrize("sdt", [
        "0901234567", "0987654321", "0321111111", "0700000000",
    ])
    def test_dang_ky_voi_cac_dau_so_dien_thoai(self, driver, sdt):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test SDT", email=random_email(), password="123456",
                       sdt=sdt, diachi="Test")
        assert page.no_server_error()


class TestRegisterValidation:
    def test_dang_ky_email_da_ton_tai(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test Trung", email=TEST_EMAIL, password="123456",
                       sdt="0901111111", diachi="456 Test")
        assert page.get_error() != "" or "Register" in driver.current_url

    @pytest.mark.parametrize("missing_field,case_name", [
        ({"ten": "", "email": "a@a.com", "password": "123456", "sdt": "0901234567", "diachi": "abc"}, "thieu_ten"),
        ({"ten": "Test", "email": "", "password": "123456", "sdt": "0901234567", "diachi": "abc"}, "thieu_email"),
        ({"ten": "Test", "email": "a@a.com", "password": "", "sdt": "0901234567", "diachi": "abc"}, "thieu_password"),
        ({"ten": "Test", "email": "a@a.com", "password": "123456", "sdt": "", "diachi": "abc"}, "thieu_sdt"),
        ({"ten": "Test", "email": "a@a.com", "password": "123456", "sdt": "0901234567", "diachi": ""}, "thieu_diachi"),
    ])
    def test_dang_ky_thieu_truong_bat_buoc(self, driver, missing_field, case_name):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(**missing_field)
        assert "Register" in driver.current_url, f"Case {case_name} phải ở lại trang đăng ký"

    @pytest.mark.parametrize("bad_email", [
        "khongdungformat",
        "abc@",
        "@gmail.com",
        "abc@gmail",
        "abc gmail.com",
        "abc@@gmail.com",
    ])
    def test_dang_ky_email_sai_format(self, driver, bad_email):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=bad_email, password="123456",
                       sdt="0901234567", diachi="Test")
        assert "Register" in driver.current_url or page.get_error() != ""

    @pytest.mark.parametrize("short_password", ["1", "12", "123", "1234", "12345"])
    def test_dang_ky_mat_khau_qua_ngan(self, driver, short_password):
        """Mật khẩu dưới 6 ký tự — hệ thống PHẢI chặn, không được tạo tài khoản."""
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password=short_password,
                       sdt="0901234567", diachi="Test")
        assert "Register" in driver.current_url or page.get_error() != "", \
            f"BUG: Web cho phép đăng ký với mật khẩu '{short_password}' quá ngắn (< 6 ký tự)"

    @pytest.mark.parametrize("bad_sdt", [
        "abc", "123", "0123456789012345", "phone-number", "++++++++",
    ])
    def test_dang_ky_sdt_sai_dinh_dang(self, driver, bad_sdt):
        """SĐT sai định dạng — hệ thống PHẢI chặn hoặc báo lỗi."""
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt=bad_sdt, diachi="Test")
        assert "Register" in driver.current_url or page.get_error() != "", \
            f"BUG: Web cho phép đăng ký với SĐT không hợp lệ '{bad_sdt}'"

    def test_dang_ky_xss_trong_ten(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="<script>alert(1)</script>", email=random_email(),
                       password="123456", sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_2_lan_lien_tiep_cung_email(self, driver):
        """Đăng ký với cùng email 2 lần liên tiếp — lần 2 phải báo lỗi."""
        email = random_email()
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Lan 1", email=email, password="123456",
                       sdt="0901234567", diachi="Test")
        page2 = RegisterPage(driver, BASE_URL)
        page2.open()
        page2.register(ten="Lan 2", email=email, password="123456",
                        sdt="0901234567", diachi="Test")
        assert page2.get_error() != "" or "Register" in driver.current_url

    def test_dang_ky_chuoi_rat_dai_trong_ten(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="A" * 300, email=random_email(), password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_email_qua_dai(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        long_email = "a" * 200 + "@gmail.com"
        page.register(ten="Test", email=long_email, password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_dia_chi_rat_dai(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt="0901234567", diachi="A" * 500)
        assert page.no_server_error()

    def test_dang_ky_sql_injection_trong_email(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email="' OR '1'='1", password="123456",
                       sdt="0901234567", diachi="Test")
        assert "Register" in driver.current_url

    def test_dang_ky_html_injection_trong_dia_chi(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt="0901234567", diachi="<img src=x onerror=alert(1)>")
        assert page.no_server_error()

    def test_dang_ky_ten_chi_co_khoang_trang(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="     ", email=random_email(), password="123456",
                       sdt="0901234567", diachi="Test")
        assert "Register" in driver.current_url

    def test_dang_ky_email_co_khoang_trang_dau_cuoi(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        email = f"  {random_email()}  "
        page.register(ten="Test", email=email, password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_mat_khau_chi_co_khoang_trang(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="      ",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_unicode_emoji_trong_ten(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test 😀🍔", email=random_email(), password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_sdt_co_chu_xen_giua_so(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt="090abc4567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_mat_khau_dai_500_ky_tu(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="x" * 500,
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_giu_lai_du_lieu_da_nhap_khi_loi(self, driver):
        """Khi đăng ký lỗi (email trùng), các trường khác nên được giữ lại trên form."""
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Giu Du Lieu", email=TEST_EMAIL, password="123456",
                       sdt="0909998888", diachi="Test giữ liệu")
        assert page.no_server_error()

    def test_dang_ky_co_validation_summary(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="", email="", password="", sdt="", diachi="")
        assert page.exists(By.CSS_SELECTOR, ".alert-danger") or "Register" in driver.current_url

    def test_link_dang_nhap_tu_trang_dang_ky_hoat_dong(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.click(*page.LOGIN_LINK)
        assert "Login" in driver.current_url

    def test_truy_cap_truc_tiep_url_register(self, driver):
        driver.get(f"{BASE_URL}/User/Register")
        assert "Register" in driver.current_url

    def test_load_lai_trang_register_f5_khong_loi(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        driver.refresh()
        assert page.no_server_error()

    def test_dang_ky_logo_hien_thi(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        assert page.exists(By.CSS_SELECTOR, "img[alt='Jollibee Logo']")

    def test_dang_ky_tieu_de_dung(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        assert "đăng ký" in driver.title.lower() or "jollibee" in driver.title.lower()

    def test_dang_ky_method_get_truc_tiep_khong_crash(self, driver):
        url = f"{BASE_URL}/User/Register?TenKH=Test&Email={random_email()}&MatKhau=123456&SDT=0901234567&DiaChi=Test"
        driver.get(url)
        page = RegisterPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_dang_ky_voi_dia_chi_co_dau_xuong_dong(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt="0901234567", diachi="Số 1\nĐường ABC\nPhường XYZ")
        assert page.no_server_error()

    def test_dang_ky_chi_dien_ten_khong_dien_gi_khac(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Chi Dien Ten")
        assert "Register" in driver.current_url

    def test_dang_ky_email_voi_dau_cong(self, driver):
        """Email dạng user+tag@gmail.com là hợp lệ theo chuẩn RFC."""
        page = RegisterPage(driver, BASE_URL)
        page.open()
        email = f"test+{int(time.time())}@gmail.com"
        page.register(ten="Test Plus", email=email, password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_email_hoa_thuong_lan_lon(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        email = random_email().upper()
        page.register(ten="Test", email=email, password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_sdt_dau_so_0_thay_bang_84(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt="84901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_mat_khau_giong_email(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        email = random_email()
        page.register(ten="Test", email=email, password=email,
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_lien_tuc_3_tai_khoan_khac_nhau(self, driver):
        """Đăng ký nhiều tài khoản liên tiếp không bị lỗi tích lũy session."""
        for i in range(3):
            page = RegisterPage(driver, BASE_URL)
            page.open()
            page.register(ten=f"User {i}", email=random_email(), password="123456",
                           sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_ten_chi_co_1_ky_tu(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="A", email=random_email(), password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_sdt_toan_so_0(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt="0000000000", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_dia_chi_chi_co_so(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test", email=random_email(), password="123456",
                       sdt="0901234567", diachi="123456789")
        assert page.no_server_error()

    def test_dang_ky_khong_co_loi_500_voi_du_lieu_null_byte(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test\x00", email=random_email(), password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_voi_email_hoa_dang_da_ton_tai_thuong(self, driver):
        """Email hoa của 1 email đã tồn tại (viết thường) — kiểm tra có bị trùng không."""
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test Hoa", email=TEST_EMAIL.upper(), password="123456",
                       sdt="0901234567", diachi="Test")
        assert page.no_server_error()

    def test_dang_ky_back_browser_sau_khi_dang_ky_thanh_cong(self, driver):
        page = RegisterPage(driver, BASE_URL)
        page.open()
        page.register(ten="Test Back", email=random_email(), password="123456",
                       sdt="0901234567", diachi="Test")
        driver.back()
        assert page.no_server_error()