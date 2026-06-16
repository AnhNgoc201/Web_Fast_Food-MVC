"""
pages/base_page.py
Base class Page Object Model — chứa các thao tác dùng chung
"""
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC


class BasePage:
    def __init__(self, driver, base_url="https://localhost:44325"):
        self.driver = driver
        self.base_url = base_url
        self.wait = WebDriverWait(driver, 10)

    def open(self, path=""):
        self.driver.get(f"{self.base_url}{path}")

    def find(self, by, value):
        return self.wait.until(EC.presence_of_element_located((by, value)))

    def find_all(self, by, value):
        try:
            return self.driver.find_elements(by, value)
        except Exception:
            return []

    def click(self, by, value):
        el = self.wait.until(EC.element_to_be_clickable((by, value)))
        try:
            el.click()
        except Exception:
            # Dùng JS click khi bị element khác che
            self.driver.execute_script("arguments[0].click();", el)

    def type(self, by, value, text):
        el = self.find(by, value)
        el.clear()
        el.send_keys(text)

    def get_text(self, by, value):
        return self.find(by, value).text

    def exists(self, by, value):
        return len(self.find_all(by, value)) > 0

    def is_visible(self, by, value):
        try:
            return self.wait.until(EC.visibility_of_element_located((by, value))).is_displayed()
        except Exception:
            return False

    def url_contains(self, fragment, timeout=10):
        try:
            WebDriverWait(self.driver, timeout).until(EC.url_contains(fragment))
            return True
        except Exception:
            return False

    def page_contains(self, text):
        return text in self.driver.page_source

    def no_server_error(self):
        """Kiểm tra trang không lỗi 500 / exception ASP.NET."""
        src = self.driver.page_source
        title = self.driver.title
        bad_markers = ["Server Error", "Exception", "Stack Trace", "HTTP Error 500"]
        return not any(m in src or m in title for m in bad_markers)