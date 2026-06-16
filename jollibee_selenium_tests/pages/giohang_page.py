"""
pages/giohang_page.py
Page Object cho DatHangController:
  /DatHang/XemGioHang, /DatHang/XacNhanDonHang, /DatHang/ThanhToan
"""
from selenium.webdriver.common.by import By
from pages.base_page import BasePage


class GioHangPage(BasePage):
    CART_ROWS      = (By.CSS_SELECTOR, "table tbody tr")
    EMPTY_ROW_TEXT = (By.XPATH, "//td[contains(text(),'Giỏ hàng của bạn đang trống')]")
    REMOVE_BTN     = (By.XPATH, "//button[contains(text(),'Xóa')]")
    INCREASE_BTN   = (By.CSS_SELECTOR, "button[value='tang']")
    DECREASE_BTN   = (By.CSS_SELECTOR, "button[value='giam']")
    QTY_INPUT      = (By.NAME, "soLuong")
    CONTINUE_SHOPPING_LINK = (By.PARTIAL_LINK_TEXT, "Tiếp tục mua sắm")
    DISCOUNT_SELECT = (By.NAME, "maGiamGia")
    CHECKOUT_LINK   = (By.PARTIAL_LINK_TEXT, "Thanh toán")

    def open(self):
        super().open("/DatHang/XemGioHang")

    def is_empty(self):
        return self.exists(*self.EMPTY_ROW_TEXT)

    def get_item_count(self):
        if self.is_empty():
            return 0
        return len(self.find_all(*self.CART_ROWS))

    def increase_first_qty(self):
        btns = self.find_all(*self.INCREASE_BTN)
        if btns:
            btns[0].click()

    def decrease_first_qty(self):
        btns = self.find_all(*self.DECREASE_BTN)
        if btns:
            btns[0].click()

    def remove_first_item(self):
        btns = self.find_all(*self.REMOVE_BTN)
        if btns:
            btns[0].click()


class XacNhanDonHangPage(BasePage):
    SUBMIT_BTN  = (By.CSS_SELECTOR, "button[type='submit'], input[type='submit']")
    SUCCESS_MSG = (By.CSS_SELECTOR, ".alert-success, .success")

    def open(self):
        super().open("/DatHang/XacNhanDonHang")