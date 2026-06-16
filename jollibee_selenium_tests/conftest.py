"""
conftest.py - Cấu hình chung cho toàn bộ bộ test Selenium
Web Jollibee - ASP.NET MVC (Web_Do_An_Nhanh)

Tích hợp sẵn plugin xuất kết quả Excel sau mỗi lần chạy.
Báo cáo được lưu vào thư mục: test_reports/
  - test_results_PASS_<timestamp>.xlsx
  - test_results_FAIL_<timestamp>.xlsx
"""

import time
import pytest
from selenium import webdriver
from selenium.webdriver.chrome.options import Options

# ── Import plugin xuất Excel ──────────────────────────────────
# Plugin đăng ký hooks tự động qua pytest_plugins
from conftest_reporter import (
    pytest_runtest_logreport,
    pytest_sessionfinish,
)

# ============================================================
# CẤU HÌNH — ĐỔI THEO MÔI TRƯỜNG CỦA BẠN
# ============================================================
BASE_URL = "https://localhost:44325"

TEST_EMAIL    = "anhngoc2xx5@gmail.com"
TEST_PASSWORD = "123456"
TEST_HOTEN    = "Phạm Ánh Ngọc"
TEST_SDT      = "0393901164"
TEST_DIACHI   = "Tây Ninh"

VALID_PRODUCT_ID  = 1
VALID_CATEGORY_ID = 1


def random_email():
    return f"test_{int(time.time() * 1000)}@gmail.com"


@pytest.fixture(scope="function")
def driver():
    """Tạo Chrome driver mới hoàn toàn cho mỗi test (không dính cookie cũ)."""
    options = Options()
    # options.add_argument("--headless")
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument("--window-size=1280,900")
    options.add_argument("--lang=vi")
    options.add_argument("--ignore-certificate-errors")
    options.add_argument("--allow-insecure-localhost")
    options.add_argument("--guest")

    d = webdriver.Chrome(options=options)
    d.implicitly_wait(8)
    d.maximize_window()
    d.delete_all_cookies()
    yield d
    d.quit()


@pytest.fixture(scope="function")
def driver_logged_in(driver):
    """Driver đã đăng nhập sẵn — dùng cho các test cần session."""
    from pages.user_page import LoginPage
    page = LoginPage(driver, BASE_URL)
    page.open()
    page.login(TEST_EMAIL, TEST_PASSWORD)
    return driver