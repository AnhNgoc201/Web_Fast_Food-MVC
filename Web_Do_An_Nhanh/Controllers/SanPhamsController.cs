using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;
using PagedList;

namespace Web_Do_An_Nhanh.Controllers
{
    public class SanPhamsController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        public ActionResult Index(int? page, int? maDM)
        {
            int pageSize = 10;   
            int pageNumber = (page ?? 1);

            var sp = db.SanPhams.AsQueryable();

            if (maDM != null)
            {
                sp = sp.Where(s => s.MaDM == maDM);
                ViewBag.TenDM = db.DanhMucs.Find(maDM)?.TenDM;
            }

            return View(sp.OrderBy(x => x.MaSP).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult TimKiem(string keyword, int? page)
        {
            int pageSize = 9;                
            int pageNumber = page ?? 1;       

            var sanpham = db.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                sanpham = sanpham.Where(x => x.TenSP.ToLower().Contains(keyword.ToLower().Trim()));
            }

            ViewBag.Keyword = keyword;

            return View("Index", sanpham.OrderBy(x => x.TenSP)
                                        .ToPagedList(pageNumber, pageSize));
        }

        public ActionResult TimKiemNangCao(string keyword, int? MaDM, int? giaMin, int? giaMax, string cay)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();
            ViewBag.Keyword = keyword;
            ViewBag.MaDM = MaDM;
            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;
            ViewBag.Cay = cay;

            var query = db.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(s => s.TenSP.ToLower().Contains(keyword.ToLower().Trim()));

            if (MaDM.HasValue)
                query = query.Where(s => s.MaDM == MaDM);

            if (giaMin.HasValue)
                query = query.Where(s => s.Gia >= giaMin);

            if (giaMax.HasValue)
                query = query.Where(s => s.Gia <= giaMax);

            if (!string.IsNullOrEmpty(cay))
            {
                if (cay == "1")
                {
                    query = query.Where(s => s.TenSP.ToLower().Contains("cay"));
                }
                else if (cay == "0")
                {
                    query = query.Where(s => !s.TenSP.ToLower().Contains("cay"));
                }
            }

            return View("TimKiemNangCao", query.ToList());
        }

        public ActionResult DropdownMenu()
        {
            var model = new DropdownViewModel
            {
                LoaiMon = db.DanhMucs.ToList()
            };

            return PartialView("_DropdownMenu", model);
        }
        public ActionResult Details(int id)
        {
            var sp = db.SanPhams.Include("DanhMuc").FirstOrDefault(x => x.MaSP == id);

            var tuongtu = db.SanPhams
                .Where(x => x.MaDM == sp.MaDM && x.MaSP != id)
                .Take(4)
                .ToList();

            ViewBag.SanPhamTuongTu = tuongtu;

            return View(sp);
        }

        public ActionResult Create()
        {
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            return View();
        }

        // POST: SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaSP,MaDM,TenSP,Gia,HinhAnh,TrangThai,MoTa")] SanPham sanPham)
        {
            if (ModelState.IsValid)
            {
                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }

        // GET: SanPhams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }

        // POST: SanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaSP,MaDM,TenSP,Gia,HinhAnh,TrangThai,MoTa")] SanPham sanPham)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sanPham).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SanPham sanPham = db.SanPhams.Find(id);
            db.SanPhams.Remove(sanPham);
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
