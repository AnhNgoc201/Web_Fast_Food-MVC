using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class ProfileController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        public ActionResult Index()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            var user = (KhachHang)Session["User"];
            return View(user); 
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        public ActionResult Edit()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            var user = (KhachHang)Session["User"];
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhachHang model)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            var user = db.KhachHangs.Find(model.MaKH);
            if (user == null)
                return HttpNotFound();

            user.TenKH = model.TenKH;
            user.SDT = model.SDT;
            user.DiaChi = model.DiaChi;

            if (!string.IsNullOrWhiteSpace(model.MatKhau))
                user.MatKhau = model.MatKhau;

            db.SaveChanges();

            Session["User"] = user;

            TempData["Message"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang != null)
            {
                db.KhachHangs.Remove(khachHang);
                db.SaveChanges();
            }

            Session["User"] = null;
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ChiTietDonHang(int? id)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var donHang = db.HoaDons.Include("ChiTietHoaDons.SanPham") 
                                     .FirstOrDefault(d => d.MaHD == id);

            if (donHang == null)
                return HttpNotFound();

            return View(donHang);
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
