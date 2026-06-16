"""
pages/sanpham_page.py
Page Object cho SanPhamsController:
  /SanPhams, /SanPhams/TimKiem, /SanPhams/TimKiemNangCao, /SanPhams/Details/id
"""
from selenium.webdriver.common.by import By
from pages.base_page import BasePage


class SanPhamPage(BasePage):
    # Header search form (toàn site)
    HEADER_SEARCH_INPUT = (By.NAME, "keyword")
    HEADER_SEARCH_BTN   = (By.CSS_SELECTOR, ".delivery-search-bar button[type='submit']")

    PRODUCT_CARDS   = (By.CSS_SELECTOR, ".product-card")
    PRODUCT_TITLE   = (By.CSS_SELECTOR, ".card-title")
    OUT_OF_STOCK    = (By.CSS_SELECTOR, ".badge.bg-secondary")
    IN_STOCK        = (By.CSS_SELECTOR, ".badge.bg-success")
    ADD_TO_CART_BTN = (By.XPATH, "//button[contains(text(),'Thêm vào Giỏ Hàng')]")
    DETAILS_LINK    = (By.PARTIAL_LINK_TEXT, "Xem Chi Tiết")
    ADVANCED_SEARCH_LINK = (By.LINK_TEXT, "TÌM MÓN PHÙ HỢP VỚI BẠN")
    PAGER           = (By.CSS_SELECTOR, ".pagination, .PagedList")
    CATEGORY_TITLE  = (By.CSS_SELECTOR, "h2.text-danger.fw-bold")

    def open(self):
        super().open("/SanPhams")

    def search_via_header(self, keyword):
        self.type(*self.HEADER_SEARCH_INPUT, keyword)
        self.click(*self.HEADER_SEARCH_BTN)

    def get_product_count(self):
        return len(self.find_all(*self.PRODUCT_CARDS))

    def add_first_available_to_cart(self):
        btns = self.find_all(*self.ADD_TO_CART_BTN)
        if btns:
            btns[0].click()
            return True
        return False

    def open_first_details(self):
        links = self.find_all(*self.DETAILS_LINK)
        if links:
            links[0].click()
            return True
        return False


class TimKiemNangCaoPage(BasePage):
    KEYWORD_INPUT = (By.NAME, "keyword")
    CATEGORY_SELECT = (By.NAME, "MaDM")
    GIA_MIN_INPUT = (By.NAME, "giaMin")
    GIA_MAX_INPUT = (By.NAME, "giaMax")
    SUBMIT_BTN = (By.CSS_SELECTOR, "button[type='submit'], input[type='submit']")

    def open(self):
        super().open("/SanPhams/TimKiemNangCao")