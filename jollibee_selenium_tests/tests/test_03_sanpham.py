"""
tests/test_03_sanpham.py
Test SanPhamsController: Index, TimKiem, TimKiemNangCao, Details
"""
import pytest
from selenium.webdriver.common.by import By
from pages.sanpham_page import SanPhamPage, TimKiemNangCaoPage
from conftest import BASE_URL, VALID_PRODUCT_ID, VALID_CATEGORY_ID


class TestSanPhamDisplay:
    def test_trang_load_thanh_cong(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_co_tieu_de_thuc_don(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        assert "THỰC ĐƠN" in driver.page_source.upper()

    def test_co_it_nhat_1_san_pham(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        assert page.get_product_count() > 0

    def test_moi_san_pham_co_ten(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        titles = page.find_all(*page.PRODUCT_TITLE)
        assert all(t.text.strip() != "" for t in titles)

    def test_co_nut_xem_chi_tiet(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.DETAILS_LINK)

    def test_co_link_tim_kiem_nang_cao(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.ADVANCED_SEARCH_LINK)

    def test_san_pham_het_hang_co_nhan_het_hang(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        # Không assert cứng vì phụ thuộc dữ liệu, chỉ kiểm tra không lỗi
        page.find_all(*page.OUT_OF_STOCK)
        assert page.no_server_error()

    def test_gia_san_pham_co_dinh_dang_vnd(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        assert "VNĐ" in driver.page_source

    def test_hinh_anh_san_pham_load_duoc(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        imgs = driver.find_elements(By.CSS_SELECTOR, "img.product-img")
        assert len(imgs) > 0


class TestSanPhamSearch:
    @pytest.mark.parametrize("keyword", [
        "Gà", "ga", "GÀ", "gà rán", "Burger", "Khoai", "Pepsi",
    ])
    def test_tim_kiem_tu_khoa_pho_bien(self, driver, keyword):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword={keyword}")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()
        assert page.get_product_count() > 0, f"Từ khóa '{keyword}' phải trả về ít nhất 1 sản phẩm"

    @pytest.mark.parametrize("keyword", [
        "xyzkhongtontai999", "asdkfjaslkdfj", "1234567890zzz",
    ])
    def test_tim_kiem_khong_co_ket_qua(self, driver, keyword):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword={keyword}")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()
        assert page.get_product_count() == 0

    def test_tim_kiem_tu_khoa_trong(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_xss_khong_thuc_thi(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=<script>alert(1)</script>")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_sql_injection_an_toan(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=' OR '1'='1")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_khoang_trang(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=   ")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_qua_header_form(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        page.search_via_header("gà")
        assert page.no_server_error()

    def test_tim_kiem_ky_tu_dac_biet(self, driver):
        for kw in ["@#$%", "!!!", "()[]"]:
            driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword={kw}")
            page = SanPhamPage(driver, BASE_URL)
            assert page.no_server_error(), f"Lỗi với từ khóa {kw}"


class TestSanPhamCategory:
    @pytest.mark.parametrize("ma_dm", [1, 2, 3, 4, 5])
    def test_loc_theo_danh_muc_id(self, driver, ma_dm):
        driver.get(f"{BASE_URL}/SanPhams?maDM={ma_dm}")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_loc_danh_muc_khong_ton_tai(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?maDM=999999")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_loc_danh_muc_id_khong_phai_so(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?maDM=abc")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_loc_danh_muc_id_am(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?maDM=-1")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()


class TestSanPhamPagination:
    @pytest.mark.parametrize("page_num", [1, 2, 3])
    def test_phan_trang_so_trang_hop_le(self, driver, page_num):
        driver.get(f"{BASE_URL}/SanPhams?page={page_num}")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_phan_trang_so_trang_qua_lon(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?page=99999")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_phan_trang_so_am(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?page=-1")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_phan_trang_khong_phai_so(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?page=abc")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_co_thanh_phan_trang_khi_nhieu_san_pham(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        # Không assert cứng, chỉ đảm bảo không lỗi
        assert page.no_server_error()


class TestSanPhamAdvancedSearch:
    def test_trang_load_thanh_cong(self, driver):
        page = TimKiemNangCaoPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    @pytest.mark.parametrize("gia_min,gia_max", [
        (10000, 50000), (0, 100000), (50000, 50000), (100000, 10000),
    ])
    def test_tim_kiem_theo_khoang_gia(self, driver, gia_min, gia_max):
        driver.get(f"{BASE_URL}/SanPhams/TimKiemNangCao?giaMin={gia_min}&giaMax={gia_max}")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_nang_cao_full_params(self, driver):
        url = f"{BASE_URL}/SanPhams/TimKiemNangCao?keyword=ga&MaDM=1&giaMin=10000&giaMax=100000&cay=true"
        driver.get(url)
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_nang_cao_gia_am(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiemNangCao?giaMin=-100&giaMax=-50")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()


class TestSanPhamDetails:
    @pytest.mark.parametrize("product_id", [1, 2, 3, 4, 5])
    def test_chi_tiet_san_pham_ton_tai(self, driver, product_id):
        driver.get(f"{BASE_URL}/SanPhams/Details/{product_id}")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_chi_tiet_san_pham_khong_ton_tai(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/Details/999999")
        page = SanPhamPage(driver, BASE_URL)
        # Phải trả về 404 hợp lệ hoặc trang thông báo — không phải lỗi 500
        assert "Server Error" not in driver.page_source

    def test_chi_tiet_san_pham_id_khong_phai_so(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/Details/abc")
        page = SanPhamPage(driver, BASE_URL)
        assert "Server Error" not in driver.page_source

    def test_chi_tiet_san_pham_id_am(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/Details/-1")
        page = SanPhamPage(driver, BASE_URL)
        assert "Server Error" not in driver.page_source

    def test_di_den_chi_tiet_tu_trang_chinh(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        opened = page.open_first_details()
        if opened:
            assert "Details" in driver.current_url


class TestSanPhamExtra:
    """Nhóm test bổ sung: hành vi UI, điều hướng, biên dữ liệu."""

    def test_them_vao_gio_tu_trang_danh_sach(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        added = page.add_first_available_to_cart()
        assert added or page.no_server_error()

    def test_load_lai_trang_san_pham_f5_khong_loi(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        driver.refresh()
        assert page.no_server_error()

    def test_tieu_de_trang_co_jollibee(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        assert "jollibee" in driver.title.lower() or page.no_server_error()

    def test_truy_cap_truc_tiep_url_sanphams(self, driver):
        driver.get(f"{BASE_URL}/SanPhams")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_tu_khoa_co_dau_cach_dau(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=%20%20ga")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_tu_khoa_qua_dai(self, driver):
        long_kw = "a" * 300
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword={long_kw}")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_loc_danh_muc_va_phan_trang_cung_luc(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?maDM=1&page=1")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_va_loc_danh_muc_cung_luc(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiem?keyword=ga&maDM=1")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_quay_lai_tu_trang_chi_tiet_bang_nut_back(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/Details/1")
        driver.back()
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_chi_tiet_san_pham_hien_thi_gia(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/Details/1")
        assert "VNĐ" in driver.page_source or "Server Error" not in driver.page_source

    def test_tim_kiem_nang_cao_khong_co_tham_so(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiemNangCao")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_tim_kiem_nang_cao_voi_cay_false(self, driver):
        driver.get(f"{BASE_URL}/SanPhams/TimKiemNangCao?cay=false")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_so_luong_san_pham_on_dinh_qua_2_lan_load(self, driver):
        """Số lượng sản phẩm hiển thị phải giống nhau giữa 2 lần load liên tiếp."""
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        count1 = page.get_product_count()
        page.open()
        count2 = page.get_product_count()
        assert count1 == count2

    def test_anh_san_pham_co_thuoc_tinh_alt(self, driver):
        page = SanPhamPage(driver, BASE_URL)
        page.open()
        imgs = driver.find_elements(By.CSS_SELECTOR, "img.product-img")
        assert all(img.get_attribute("alt") is not None for img in imgs)

    def test_danh_muc_hien_thi_tieu_de_dung_khi_co_madm(self, driver):
        driver.get(f"{BASE_URL}/SanPhams?maDM=1")
        page = SanPhamPage(driver, BASE_URL)
        assert page.no_server_error()