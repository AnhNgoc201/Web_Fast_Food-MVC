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
    public class TinhTrangsController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        private NhanVien LayNhanVienDangNhap()
        {
            return Session["NhanVien"] as NhanVien;
        }

        private bool ChiChoNhanVien()
        {
            return LayNhanVienDangNhap() != null;
        }

        private bool ChiChoAdmin()
        {
            var nv = LayNhanVienDangNhap();
            return nv != null && nv.MaVT == 1;
        }

        public ActionResult Index()
        {
            return View(db.TinhTrangs.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TinhTrang tt = db.TinhTrangs.Find(id);
            if (tt == null)
                return HttpNotFound();

            return View(tt);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaTT,TenTT")] TinhTrang tinhTrang)
        {
            if (ModelState.IsValid)
            {
                db.TinhTrangs.Add(tinhTrang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tinhTrang);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TinhTrang tt = db.TinhTrangs.Find(id);
            if (tt == null)
                return HttpNotFound();

            return View(tt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaTT,TenTT")] TinhTrang tinhTrang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tinhTrang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tinhTrang);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TinhTrang tt = db.TinhTrangs.Find(id);
            if (tt == null)
                return HttpNotFound();

            return View(tt);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TinhTrang tt = db.TinhTrangs.Find(id);
            db.TinhTrangs.Remove(tt);
            db.SaveChanges();
            return RedirectToAction("Index");
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