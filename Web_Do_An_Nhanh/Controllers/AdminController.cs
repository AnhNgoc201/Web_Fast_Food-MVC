using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class AdminController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        private bool KiemTraNhanVien()
        {
            return Session["NhanVien"] != null;
        }

        public ActionResult Index()
        {
            if (!KiemTraNhanVien())
                return RedirectToAction("Login", "NhanVien");
            ViewBag.TongKhachHang = db.KhachHangs.Count();
            ViewBag.TongNhanVien = db.NhanViens.Count();
            ViewBag.TongSanPham = db.SanPhams.Count();
            ViewBag.TongHoaDon = db.HoaDons.Count();
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, 1).AddMonths(-5);

            var query = db.HoaDons
                          .Where(h => h.NgayLap >= start && h.NgayLap <= now)
                          .GroupBy(h => new { h.NgayLap.Value.Year, h.NgayLap.Value.Month })
                          .Select(g => new
                          {
                              Nam = g.Key.Year,
                              Thang = g.Key.Month,
                              TongTien = g.Sum(x => (decimal?)x.TongTien) ?? 0,
                              SoDon = g.Count()
                          })
                          .ToList();

            var months = Enumerable.Range(0, 6)
                                   .Select(i => start.AddMonths(i))
                                   .ToList();

            var labels = new List<string>();
            var doanhThu = new List<string>();
            var soDon = new List<string>();

            foreach (var m in months)
            {
                labels.Add("'T" + m.Month + "'");

                var dataMonth = query.FirstOrDefault(x => x.Nam == m.Year && x.Thang == m.Month);
                decimal tongTien = dataMonth != null ? dataMonth.TongTien : 0;
                int count = dataMonth != null ? dataMonth.SoDon : 0;
                decimal trieu = tongTien / 1000000m;
                doanhThu.Add(trieu.ToString("0.##", CultureInfo.InvariantCulture));
                soDon.Add(count.ToString());
            }

            ViewBag.ChartLabels = "[" + string.Join(",", labels) + "]";
            ViewBag.ChartDoanhThu = "[" + string.Join(",", doanhThu) + "]";
            ViewBag.ChartSoDon = "[" + string.Join(",", soDon) + "]";

            var topSpRaw = db.ChiTietHoaDons
                             .GroupBy(c => c.MaSP)
                             .Select(g => new
                             {
                                 MaSP = g.Key,
                                 SoLuong = g.Sum(x => x.SoLuong)
                             })
                             .OrderByDescending(x => x.SoLuong)
                             .Take(4)
                             .ToList();

            var tenSp = new List<string>();
            var soLuongSp = new List<string>();

            foreach (var item in topSpRaw)
            {
                var sp = db.SanPhams.Find(item.MaSP);
                if (sp != null)
                {
                    tenSp.Add("'" + sp.TenSP.Replace("'", "\\'") + "'");
                    soLuongSp.Add(item.SoLuong.ToString());
                }
            }

            if (!tenSp.Any())
            {
                tenSp.Add("'Chưa có dữ liệu'");
                soLuongSp.Add("0");
            }

            ViewBag.TopSpLabels = "[" + string.Join(",", tenSp) + "]";
            ViewBag.TopSpData = "[" + string.Join(",", soLuongSp) + "]";

            return View();
        }
    }
}
