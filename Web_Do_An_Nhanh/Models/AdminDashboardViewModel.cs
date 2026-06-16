using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Do_An_Nhanh.Models
{
    public class DoanhThuThangViewModel
    {
        public string Thang { get; set; }       
        public decimal DoanhThu { get; set; }   
        public int SoDonHang { get; set; }     
    }

    public class SanPhamBanChayViewModel
    {
        public string TenSP { get; set; }
        public int SoLuongBan { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TongKhachHang { get; set; }
        public int TongNhanVien { get; set; }
        public int TongSanPham { get; set; }
        public int TongDonHang { get; set; }

        public List<DoanhThuThangViewModel> DoanhThuTheoThang { get; set; }

        public List<SanPhamBanChayViewModel> SanPhamBanChay { get; set; }
    }
}