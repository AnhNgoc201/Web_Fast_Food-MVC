using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class DatHangController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ApDungMaGiamGia(int maGiamGia)
        {
            var gh = Session["gh"] as GioHang;
            if (gh == null || gh.lst == null || !gh.lst.Any())
            {
                TempData["ThongBao"] = "Giỏ hàng trống!";
                return RedirectToAction("XemGioHang");
            }

            var km = db.KhuyenMais.FirstOrDefault(x => x.MaKM == maGiamGia
                                                       && x.NgayBatDau <= DateTime.Now
                                                       && x.NgayKetThuc >= DateTime.Now);
            if (km != null)
            {
                gh.PhanTramGiam = (float)(km.PhanTramGiam ?? 0);
                gh.TenKM = km.TenKM;
                gh.MaKM = km.MaKM; 
                TempData["ThongBao"] = $"Áp dụng mã '{km.TenKM}' giảm {km.PhanTramGiam}% thành công!";
            }
            else
            {
                gh.PhanTramGiam = 0;
                gh.TenKM = null;
                gh.MaKM = null;
                TempData["ThongBao"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn!";
            }

            Session["gh"] = gh;
            return RedirectToAction("XemGioHang");
        }

        public ActionResult ThemMatHang(int msp, int soluong)
        {
            GioHang gh = Session["gh"] as GioHang ?? new GioHang();

            gh.Them(msp, soluong);

            Session["gh"] = gh;
            Session["SoLuongGioHang"] = gh.TongSLHang();
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng thành công!";
            return RedirectToAction("Index", "SanPhams");
        }


        public ActionResult XemGioHang()
        {
            GioHang gh = Session["gh"] as GioHang ?? new GioHang();
            ViewBag.MaGiamGiaList = db.KhuyenMais
                .Where(km => km.NgayBatDau <= DateTime.Now && km.NgayKetThuc >= DateTime.Now)
                .ToList();
            return View(gh);
        }
        [HttpPost]
        public ActionResult CapNhatSoLuong(int msp, int soLuong, string hanhDong)
        {
            GioHang gh = Session["gh"] as GioHang;
            if (gh != null && gh.lst != null)
            {
                var item = gh.lst.FirstOrDefault(x => x.iMaSP == msp);
                if (item != null)
                {
                    if (hanhDong == "tang")
                        item.iSoLuong++;
                    else if (hanhDong == "giam" && item.iSoLuong > 1)
                        item.iSoLuong--;
                    else
                        item.iSoLuong = soLuong; 

                    Session["gh"] = gh;
                }
            }
            return RedirectToAction("XemGioHang");
        }
        public ActionResult XoaMatHang(int msp)
        {
            GioHang gh = Session["gh"] as GioHang ?? new GioHang();
            gh.Xoa(msp);
            Session["gh"] = gh;
            Session["SoLuongGioHang"] = gh.TongSLHang();
            return RedirectToAction("XemGioHang");
        }

        public ActionResult XacNhanDonHang()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            var user = (KhachHang)Session["User"];
            ViewBag.KhachHang = user;

            GioHang gh = Session["gh"] as GioHang;
            if (gh == null || !gh.lst.Any())
                return RedirectToAction("Index", "SanPhams");

            return View(gh);
        }

        public ActionResult ThanhToan()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            var user = (KhachHang)Session["User"];
            GioHang gh = Session["gh"] as GioHang;

            if (gh == null || !gh.lst.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("XemGioHang", "DatHang");
            }

            decimal tongTienHang = gh.TongThanhTien();
            decimal giamGia = tongTienHang * ((decimal)gh.PhanTramGiam / 100);
            decimal phiShip = gh.PhiShip ? 10000m : 0m;
            decimal thanhToan = tongTienHang - giamGia + phiShip;

            int? maKM = null;
            if (gh.MaKM.HasValue)
            {
                var km = db.KhuyenMais.FirstOrDefault(x => x.MaKM == gh.MaKM.Value
                                                           && x.NgayBatDau <= DateTime.Now
                                                           && x.NgayKetThuc >= DateTime.Now);
                if (km != null) maKM = km.MaKM;
            }

            HoaDon hd = new HoaDon
            {
                MaKH = user.MaKH,
                NgayLap = DateTime.Now,
                TongTien = thanhToan,
                DiaChi = user.DiaChi,
                MaTT = 2,
                MaNV = null,
                MaKM = maKM,   
                PhiShip = gh.PhiShip
            };

            db.HoaDons.Add(hd);
            db.SaveChanges();

            foreach (var item in gh.lst)
            {
                ChiTietHoaDon cthd = new ChiTietHoaDon
                {
                    MaHD = hd.MaHD,
                    MaSP = item.iMaSP,
                    SoLuong = item.iSoLuong,
                    DonGia = (decimal)item.dDonGia
                };
                db.ChiTietHoaDons.Add(cthd);
            }

            db.SaveChanges();

            Session["gh"] = null;
            Session["SoLuongGioHang"] = 0;

            TempData["SuccessMessage"] = "🎉 Thanh toán thành công! Cảm ơn bạn đã mua hàng.";
            return RedirectToAction("Index", "SanPhams");
        }
    }
}
