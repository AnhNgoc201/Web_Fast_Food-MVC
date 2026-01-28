using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class ChiTietHoaDonsController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        private NhanVien LayNhanVienDangNhap()
        {
            return Session["NhanVien"] as NhanVien;
        }

        private bool ChiChoAdmin()
        {
            var nv = LayNhanVienDangNhap();
            return nv != null && nv.MaVT == 1 || nv.MaVT == 2;
        }

        private ActionResult KhongCoQuyen()
        {
            TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Index(int? maHD)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var chiTiet = db.ChiTietHoaDons
                            .Include(c => c.HoaDon)
                            .Include(c => c.SanPham);

            if (maHD.HasValue)
            {
                chiTiet = chiTiet.Where(c => c.MaHD == maHD.Value);
            }

            ViewBag.MaHDDangXem = maHD;

            return View(chiTiet.ToList());
        }

        public ActionResult Details(int? maHD, int? maSP)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (maHD == null || maSP == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ChiTietHoaDon ct = db.ChiTietHoaDons.Find(maHD, maSP);
            if (ct == null)
                return HttpNotFound();

            return View(ct);
        }

        public ActionResult Create(int? maHD)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            ViewBag.MaHD = new SelectList(db.HoaDons, "MaHD", "MaHD", maHD);
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP");
            ViewBag.MaHDDangChon = maHD;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaHD,MaSP,SoLuong,DonGia")] ChiTietHoaDon chiTietHoaDon)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (ModelState.IsValid)
            {
                db.ChiTietHoaDons.Add(chiTietHoaDon);
                db.SaveChanges();
                return RedirectToAction("Index", new { maHD = chiTietHoaDon.MaHD });
            }

            ViewBag.MaHD = new SelectList(db.HoaDons, "MaHD", "MaHD", chiTietHoaDon.MaHD);
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP", chiTietHoaDon.MaSP);
            ViewBag.MaHDDangChon = chiTietHoaDon.MaHD;

            return View(chiTietHoaDon);
        }

        public ActionResult Edit(int? maHD, int? maSP)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (maHD == null || maSP == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ChiTietHoaDon ct = db.ChiTietHoaDons.Find(maHD, maSP);
            if (ct == null)
                return HttpNotFound();

            ViewBag.MaHD = new SelectList(db.HoaDons, "MaHD", "MaHD", ct.MaHD);
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP", ct.MaSP);
            ViewBag.MaHDDangChon = ct.MaHD;

            return View(ct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHD,MaSP,SoLuong,DonGia")] ChiTietHoaDon chiTietHoaDon)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (ModelState.IsValid)
            {
                db.Entry(chiTietHoaDon).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { maHD = chiTietHoaDon.MaHD });
            }

            ViewBag.MaHD = new SelectList(db.HoaDons, "MaHD", "MaHD", chiTietHoaDon.MaHD);
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP", chiTietHoaDon.MaSP);
            ViewBag.MaHDDangChon = chiTietHoaDon.MaHD;

            return View(chiTietHoaDon);
        }

        public ActionResult Delete(int? maHD, int? maSP)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (maHD == null || maSP == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ChiTietHoaDon ct = db.ChiTietHoaDons
                                 .Include(c => c.SanPham)
                                 .Include(c => c.HoaDon)
                                 .FirstOrDefault(c => c.MaHD == maHD && c.MaSP == maSP);
            if (ct == null)
                return HttpNotFound();

            return View(ct);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int maHD, int maSP)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            ChiTietHoaDon ct = db.ChiTietHoaDons.Find(maHD, maSP);
            db.ChiTietHoaDons.Remove(ct);
            db.SaveChanges();
            return RedirectToAction("Index", new { maHD = maHD });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
