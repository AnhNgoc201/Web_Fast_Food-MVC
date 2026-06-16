"""
pages/user_page.py
Page Object cho UserController:
  /User/Login, /User/Register, /User/Logout, /User/LichSuMuaHang,
  /User/ForgotPassword, /User/SendOtp, /User/VerifyOtp, /User/ResetPassword

GHI CHÚ QUAN TRỌNG:
  Layout (_Layout.cshtml) có form tìm kiếm ở header với button[type=submit]
  riêng. Vì vậy KHÔNG dùng "button[type='submit']" chung cho cả trang —
  phải định vị nút submit nằm CÙNG FORM với input Email/Password/v.v.
"""
from selenium.webdriver.common.by import By
from pages.base_page import BasePage


class LoginPage(BasePage):
    EMAIL_INPUT    = (By.NAME, "Email")
    PASSWORD_INPUT = (By.NAME, "Password")
    SUBMIT_BTN     = (By.XPATH, "//input[@name='Password']/ancestor::form//button[@type='submit']")
    ERROR_MSG      = (By.CSS_SELECTOR, "p.text-danger, .alert-danger")
    SUCCESS_MSG    = (By.CSS_SELECTOR, ".alert-success")
    FORGOT_LINK    = (By.LINK_TEXT, "Quên mật khẩu?")
    REGISTER_LINK  = (By.LINK_TEXT, "Đăng ký ngay")
    STAFF_LOGIN_LINK = (By.LINK_TEXT, "Đăng nhập nhân viên")

    def open(self):
        super().open("/User/Login")

    def login(self, email, password):
        self.type(*self.EMAIL_INPUT, email)
        self.type(*self.PASSWORD_INPUT, password)
        self.click(*self.SUBMIT_BTN)

    def get_error(self):
        try:
            return self.find(*self.ERROR_MSG).text
        except Exception:
            return ""

    def get_success(self):
        try:
            return self.find(*self.SUCCESS_MSG).text
        except Exception:
            return ""


class RegisterPage(BasePage):
    TEN_INPUT    = (By.NAME, "TenKH")
    EMAIL_INPUT  = (By.NAME, "Email")
    SDT_INPUT    = (By.NAME, "SDT")
    PASS_INPUT   = (By.NAME, "MatKhau")
    DIACHI_INPUT = (By.NAME, "DiaChi")
    SUBMIT_BTN   = (By.XPATH, "//textarea[@name='DiaChi']/ancestor::form//button[@type='submit']")
    ERROR_MSG    = (By.CSS_SELECTOR, ".alert-danger")
    FIELD_VALIDATION = (By.CSS_SELECTOR, ".text-danger.small, .field-validation-error")
    LOGIN_LINK   = (By.LINK_TEXT, "Đăng nhập ngay")

    def open(self):
        super().open("/User/Register")

    def register(self, ten="", email="", password="", sdt="", diachi=""):
        if ten:
            self.type(*self.TEN_INPUT, ten)
        if email:
            self.type(*self.EMAIL_INPUT, email)
        if sdt:
            self.type(*self.SDT_INPUT, sdt)
        if password:
            self.type(*self.PASS_INPUT, password)
        if diachi:
            self.type(*self.DIACHI_INPUT, diachi)
        self.click(*self.SUBMIT_BTN)

    def get_error(self):
        try:
            return self.find(*self.ERROR_MSG).text
        except Exception:
            return ""

    def get_field_errors(self):
        return [e.text for e in self.find_all(*self.FIELD_VALIDATION) if e.text.strip()]


class ForgotPasswordPage(BasePage):
    EMAIL_INPUT = (By.NAME, "email")
    SUBMIT_BTN  = (By.XPATH, "//input[@name='email']/ancestor::form//button[@type='submit']")
    ERROR_MSG   = (By.CSS_SELECTOR, ".text-danger, .alert-danger")

    def open(self):
        super().open("/User/ForgotPassword")

    def send_otp(self, email):
        self.type(*self.EMAIL_INPUT, email)
        self.click(*self.SUBMIT_BTN)

    def get_error(self):
        try:
            return self.find(*self.ERROR_MSG).text
        except Exception:
            return ""