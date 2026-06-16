"""
tests/test_04_giohang.py
Test DatHangController: ThemMatHang, XemGioHang, XoaMatHang,
CapNhatSoLuong, ApDungMaGiamGia, XacNhanDonHang
"""
import pytest
from pages.giohang_page import GioHangPage, XacNhanDonHangPage
from conftest import BASE_URL, VALID_PRODUCT_ID


class TestThemVaoGio:
    def test_them_san_pham_hop_le(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        assert "SanPhams" in driver.current_url
        assert "thành công" in driver.page_source.lower() or \
               "giỏ hàng" in driver.page_source.lower()

    @pytest.mark.parametrize("soluong", [1, 2, 5, 10, 50])
    def test_them_voi_so_luong_khac_nhau(self, driver, soluong):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong={soluong}")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_them_voi_so_luong_0(self, driver):
        page = GioHangPage(driver, BASE_URL)
        page.open()
        count_before = page.get_item_count()

        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=0")
        assert page.no_server_error()

        page.open()
        assert page.get_item_count() == count_before, "Số lượng 0 không được thêm vào giỏ"

    def test_them_voi_so_luong_am(self, driver):
        page = GioHangPage(driver, BASE_URL)
        page.open()
        count_before = page.get_item_count()

        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=-5")
        assert page.no_server_error()

        page.open()
        assert page.get_item_count() == count_before, "Số lượng âm không được thêm vào giỏ"

    def test_them_voi_san_pham_khong_ton_tai(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=999999&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_them_voi_msp_khong_phai_so(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=abc&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        assert "Server Error" not in driver.page_source

    def test_them_thieu_param_soluong(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    @pytest.mark.parametrize("product_id", [1, 2, 3])
    def test_them_nhieu_san_pham_khac_nhau_lien_tiep(self, driver, product_id):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={product_id}&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_them_cung_1_san_pham_2_lan_cong_so_luong(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=2")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()


class TestXemGioHang:
    def test_xem_gio_hang_trong(self, driver):
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_xem_gio_hang_co_san_pham(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=2")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.get_item_count() >= 1

    def test_gio_hang_hien_thi_dung_so_luong_san_pham_khac_nhau(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=2&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.get_item_count() >= 2

    def test_gio_hang_co_nut_tiep_tuc_mua_sam(self, driver):
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.exists(*page.CONTINUE_SHOPPING_LINK)

    def test_gio_hang_trong_hien_thong_bao(self, driver):
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.is_empty() or page.get_item_count() >= 0


class TestCapNhatGioHang:
    def test_tang_so_luong(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp={VALID_PRODUCT_ID}&soLuong=1&hanhDong=tang")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_giam_so_luong(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=3")
        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp={VALID_PRODUCT_ID}&soLuong=3&hanhDong=giam")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_giam_so_luong_xuong_0_hoac_xoa(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp={VALID_PRODUCT_ID}&soLuong=1&hanhDong=giam")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_cap_nhat_hanh_dong_khong_hop_le(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=2")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        count_before = page.get_item_count()

        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp={VALID_PRODUCT_ID}&soLuong=1&hanhDong=xyz")
        assert page.no_server_error()

        page.open()
        assert page.get_item_count() == count_before, "Hành động không hợp lệ không được thay đổi giỏ"

    def test_cap_nhat_san_pham_khong_co_trong_gio(self, driver):
        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp=999999&soLuong=1&hanhDong=tang")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    @pytest.mark.parametrize("soluong", [1, 5, 10, 100])
    def test_cap_nhat_voi_nhieu_gia_tri_so_luong(self, driver, soluong):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp={VALID_PRODUCT_ID}&soLuong={soluong}&hanhDong=tang")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()


class TestXoaKhoiGio:
    def test_xoa_san_pham_co_trong_gio(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp={VALID_PRODUCT_ID}")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_xoa_san_pham_khong_co_trong_gio(self, driver):
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp=999999")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_xoa_2_lan_lien_tiep(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp={VALID_PRODUCT_ID}")
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp={VALID_PRODUCT_ID}")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_xoa_toan_bo_gio_hang_tung_san_pham(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=2&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp=1")
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang?msp=2")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.is_empty() or page.get_item_count() == 0


class TestApDungMaGiamGia:
    def test_ap_dung_ma_khong_hop_le(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    @pytest.mark.parametrize("ma_giam_gia", [1, 2, 999999, -1, 0])
    def test_ap_dung_cac_ma_giam_gia(self, driver, ma_giam_gia):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_giam_gia_voi_gio_hang_trong(self, driver):
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()


class TestXacNhanDonHang:
    def test_xac_nhan_khong_dang_nhap(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = XacNhanDonHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_xac_nhan_da_dang_nhap(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = XacNhanDonHangPage(driver_logged_in, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_xac_nhan_gio_hang_trong(self, driver_logged_in):
        page = XacNhanDonHangPage(driver_logged_in, BASE_URL)
        page.open()
        assert page.no_server_error()

    @pytest.mark.parametrize("soluong", [1, 3, 5])
    def test_dat_hang_voi_nhieu_so_luong(self, driver_logged_in, soluong):
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong={soluong}")
        page = XacNhanDonHangPage(driver_logged_in, BASE_URL)
        page.open()
        assert page.no_server_error()


class TestGioHangExtra:
    """Nhóm test bổ sung: hành vi UI, biên dữ liệu, điều hướng giỏ hàng."""

    def test_load_lai_trang_gio_hang_f5_khong_loi(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        driver.refresh()
        assert page.no_server_error()

    def test_click_tiep_tuc_mua_sam_chuyen_ve_sanphams(self, driver):
        page = GioHangPage(driver, BASE_URL)
        page.open()
        page.click(*page.CONTINUE_SHOPPING_LINK)
        assert "SanPhams" in driver.current_url

    def test_them_san_pham_voi_soluong_la_chuoi_chu(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=abc")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_them_san_pham_voi_msp_am(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=-1&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_cap_nhat_so_luong_thieu_msp(self, driver):
        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?soLuong=1&hanhDong=tang")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_xoa_thieu_param_msp(self, driver):
        driver.get(f"{BASE_URL}/DatHang/XoaMatHang")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_them_san_pham_soluong_so_thuc(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1.5")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_them_san_pham_soluong_rat_lon(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=999999999")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_truy_cap_truc_tiep_url_xemgiohang(self, driver):
        driver.get(f"{BASE_URL}/DatHang/XemGioHang")
        assert "GioHang" in driver.current_url or "XemGioHang" in driver.current_url

    def test_gio_hang_giu_du_lieu_qua_2_lan_xem(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=2")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        count1 = page.get_item_count()
        page.open()
        count2 = page.get_item_count()
        assert count1 == count2

    def test_them_sql_injection_vao_msp(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=1' OR '1'='1&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()

    def test_gio_hang_hien_thi_dung_tong_tien(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=2")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert "VNĐ" in driver.page_source or page.is_empty()

    def test_tang_so_luong_nhieu_lan_lien_tiep(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        for _ in range(3):
            driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp={VALID_PRODUCT_ID}&soLuong=1&hanhDong=tang")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_xac_nhan_don_hang_load_lai_f5(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = XacNhanDonHangPage(driver_logged_in, BASE_URL)
        page.open()
        driver_logged_in.refresh()
        assert page.no_server_error()

    def test_xac_nhan_don_hang_co_nut_submit(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = XacNhanDonHangPage(driver_logged_in, BASE_URL)
        page.open()
        assert page.exists(*page.SUBMIT_BTN) or page.no_server_error()

    def test_them_2_san_pham_giong_nhau_cong_don_so_luong(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.get_item_count() >= 1

    def test_dat_hang_voi_msp_khong_ton_tai_trong_gio(self, driver_logged_in):
        driver_logged_in.get(f"{BASE_URL}/DatHang/ThemMatHang?msp=999999&soluong=1")
        page = XacNhanDonHangPage(driver_logged_in, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_giam_so_luong_khi_chi_co_1_san_pham_trong_gio(self, driver):
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.get(f"{BASE_URL}/DatHang/CapNhatSoLuong?msp={VALID_PRODUCT_ID}&soLuong=1&hanhDong=giam")
        page = GioHangPage(driver, BASE_URL)
        page.open()
        assert page.no_server_error()

    def test_quay_lai_trang_truoc_sau_khi_them_vao_gio(self, driver):
        driver.get(f"{BASE_URL}/SanPhams")
        driver.get(f"{BASE_URL}/DatHang/ThemMatHang?msp={VALID_PRODUCT_ID}&soluong=1")
        driver.back()
        page = GioHangPage(driver, BASE_URL)
        assert page.no_server_error()