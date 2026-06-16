using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Globalization;
using Web_Do_An_Nhanh.Models;
using System.Data.Entity.Validation;

namespace Web_Do_An_Nhanh.Controllers
{
    public class SanPhamAdminController : Controller
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

        private ActionResult KhongCoQuyen()
        {
            var nv = LayNhanVienDangNhap();
            if (nv == null)
            {
                return RedirectToAction("Login", "NhanVien");
            }

            TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
            return RedirectToAction("Index", "Admin");
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!ChiChoNhanVien())
            {
                filterContext.Result = RedirectToAction("Login", "NhanVien");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // validate sản phẩm
        private void ValidateSanPhamInput(SanPham sp)
        {
            if (sp == null)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin trước khi lưu.");
                return;
            }

            sp.TenSP = sp.TenSP != null ? sp.TenSP.Trim() : null;
            sp.MoTa = sp.MoTa != null ? sp.MoTa.Trim() : null;

            if (sp.MaDM <= 0)
                ModelState.AddModelError("MaDM", "Vui lòng chọn danh mục.");

            if (string.IsNullOrWhiteSpace(sp.TenSP))
                ModelState.AddModelError("TenSP", "Vui lòng nhập tên sản phẩm.");

            if (sp.Gia <= 0)
                ModelState.AddModelError("Gia", "Vui lòng nhập giá hợp lệ (> 0).");

            if (string.IsNullOrWhiteSpace(sp.MoTa))
                ModelState.AddModelError("MoTa", "Vui lòng nhập mô tả.");

            if (!ModelState.IsValid)
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin trước khi lưu.");
        }
        private void AddEfValidationErrorsToModelState(DbEntityValidationException ex)
        {
            foreach (var eve in ex.EntityValidationErrors)
            {
                foreach (var ve in eve.ValidationErrors)
                {
                    ModelState.AddModelError(ve.PropertyName, ve.ErrorMessage);
                }
            }
            ModelState.AddModelError("", "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.");
        }


        public ActionResult Index(string searchString, int? maDM, bool? trangThai, int? page)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var sanPhams = db.SanPhams
                             .Include(s => s.DanhMuc)
                             .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                sanPhams = sanPhams.Where(s =>
                    (s.TenSP != null && s.TenSP.Contains(searchString)) ||
                    (s.MoTa != null && s.MoTa.Contains(searchString)));
            }

            if (maDM.HasValue && maDM.Value > 0)
            {
                sanPhams = sanPhams.Where(s => s.MaDM == maDM.Value);
            }

            if (trangThai.HasValue)
            {
                sanPhams = sanPhams.Where(s => s.TrangThai == trangThai.Value);
            }

            sanPhams = sanPhams.OrderByDescending(s => s.MaSP);

            int pageSize = 10;
            int pageNumber = page ?? 1;

            int totalItems = sanPhams.Count();
            var items = sanPhams
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentMaDM = maDM;
            ViewBag.CurrentTrangThai = trangThai;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.DanhMucList = new SelectList(db.DanhMucs.ToList(), "MaDM", "TenDM");

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SanPham sanPham = db.SanPhams
                                .Include(s => s.DanhMuc)
                                .FirstOrDefault(s => s.MaSP == id);

            if (sanPham == null)
                return HttpNotFound();

            return View(sanPham);
        }

        public ActionResult Create()
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
        [Bind(Include = "MaSP,MaDM,TenSP,Gia,TrangThai,MoTa")] SanPham sanPham,
        HttpPostedFileBase HinhAnhFile)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            ValidateSanPhamInput(sanPham);

            if (!ModelState.IsValid)
            {
                ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
                return View(sanPham);
            }

            if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
            {
                string fileName = Path.GetFileName(HinhAnhFile.FileName);
                string folder = Server.MapPath("~/Content/Images");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string path = Path.Combine(folder, fileName);
                HinhAnhFile.SaveAs(path);
                sanPham.HinhAnh = fileName;
            }

            try
            {
                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException ex)
            {
                AddEfValidationErrorsToModelState(ex);
            }

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }


        public ActionResult Edit(int? id)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
                return HttpNotFound();

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaSP,MaDM,TenSP,Gia,TrangThai,MoTa")] SanPham sanPham, HttpPostedFileBase HinhAnhFile)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var sp = db.SanPhams.Find(sanPham.MaSP);
            if (sp == null) return HttpNotFound();

            // validate bắt buộc nhập
            ValidateSanPhamInput(sanPham);

            if (!ModelState.IsValid)
            {
                ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
                return View(sanPham);
            }

            sp.TenSP = sanPham.TenSP;
            sp.Gia = sanPham.Gia;
            sp.MoTa = sanPham.MoTa;
            sp.MaDM = sanPham.MaDM;
            sp.TrangThai = sanPham.TrangThai;

            if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
            {
                string fileName = Path.GetFileName(HinhAnhFile.FileName);
                string folder = Server.MapPath("~/Content/Images");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string path = Path.Combine(folder, fileName);
                HinhAnhFile.SaveAs(path);

                sp.HinhAnh = fileName;
            }

            try
            {
                db.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException ex)
            {
                AddEfValidationErrorsToModelState(ex);
            }

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }




        public ActionResult Delete(int? id)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SanPham sanPham = db.SanPhams
                                .Include(s => s.DanhMuc)
                                .FirstOrDefault(s => s.MaSP == id);

            if (sanPham == null)
                return HttpNotFound();

            return View(sanPham);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                db.SanPhams.Remove(sanPham);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleTrangThai(int id, string searchString, int? maDM, bool? trangThai, int? page)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var sp = db.SanPhams.Find(id);
            if (sp == null)
            {
                return HttpNotFound();
            }

            bool hienTai = sp.TrangThai.HasValue ? sp.TrangThai.Value : false;
            sp.TrangThai = !hienTai;
            db.SaveChanges();
            return RedirectToAction("Index", new
            {
                searchString = searchString,
                maDM = maDM,
                trangThai = trangThai,
                page = page
            });
        }


        // =================== EXPORT / IMPORT ===================
        public ActionResult Export(string searchString, int? maDM, bool? trangThai)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var query = db.SanPhams.Include(s => s.DanhMuc).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    (s.TenSP != null && s.TenSP.Contains(searchString)) ||
                    (s.MoTa != null && s.MoTa.Contains(searchString)));
            }

            if (maDM.HasValue && maDM.Value > 0)
            {
                query = query.Where(s => s.MaDM == maDM.Value);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(s => s.TrangThai == trangThai.Value);
            }

            var list = query.OrderByDescending(s => s.MaSP).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("MaSP,MaDM,TenSP,Gia,HinhAnh,TrangThai,MoTa");

            foreach (var sp in list)
            {
                sb.AppendLine(string.Join(",",
                    sp.MaSP,
                    sp.MaDM,
                    EscapeForCsv(sp.TenSP),
                    sp.Gia.ToString(CultureInfo.InvariantCulture),
                    EscapeForCsv(sp.HinhAnh),
                    //(bool)sp.TrangThai ? "1" : "0",
                    (sp.TrangThai.HasValue && sp.TrangThai.Value) ? "1" : "0",
                    EscapeForCsv(sp.MoTa)
                ));
            }

            var encoding = new UTF8Encoding(true);
            byte[] buffer = encoding.GetBytes(sb.ToString());

            return File(buffer, "text/csv", "SanPham.csv");
        }

        public ActionResult Import()
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file cần import.");
                return View();
            }

            int inserted = 0;
            int updated = 0;

            using (var reader = new StreamReader(file.InputStream, Encoding.UTF8))
            {
                string line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var cols = ParseCsvLine(line);
                    if (cols.Count < 6)
                        continue;

                    int maSp;
                    int.TryParse(cols[0].Trim(), out maSp);
                    int maDm;
                    int.TryParse(cols[1].Trim(), out maDm);
                    string tenSp = cols[2].Trim();
                    decimal gia;
                    decimal.TryParse(cols[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out gia);
                    string hinhAnh = cols[4].Trim();
                    string trangThaiStr = cols[5].Trim();
                    string moTa = cols.Count >= 7 ? cols[6].Trim() : null;

                    if (maDm <= 0 || string.IsNullOrWhiteSpace(tenSp) || gia <= 0)
                        continue;

                    bool trangThai = true;
                    if (trangThaiStr == "0" || string.Equals(trangThaiStr, "false", StringComparison.OrdinalIgnoreCase))
                        trangThai = false;

                    if (maSp > 0)
                    {
                        var existing = db.SanPhams.Find(maSp);
                        if (existing != null)
                        {
                            existing.MaDM = maDm;
                            existing.TenSP = tenSp;
                            existing.Gia = gia;
                            existing.HinhAnh = hinhAnh;
                            existing.TrangThai = trangThai;
                            existing.MoTa = moTa;
                            updated++;
                        }
                        else
                        {
                            var sp = new SanPham
                            {
                                MaDM = maDm,
                                TenSP = tenSp,
                                Gia = gia,
                                HinhAnh = hinhAnh,
                                TrangThai = trangThai,
                                MoTa = moTa
                            };
                            db.SanPhams.Add(sp);
                            inserted++;
                        }
                    }
                    else
                    {
                        var sp = new SanPham
                        {
                            MaDM = maDm,
                            TenSP = tenSp,
                            Gia = gia,
                            HinhAnh = hinhAnh,
                            TrangThai = trangThai,
                            MoTa = moTa
                        };
                        db.SanPhams.Add(sp);
                        inserted++;
                    }
                }
            }

            db.SaveChanges();

            TempData["ImportMessage"] =
                $"Import thành công. Thêm {inserted} sản phẩm mới, cập nhật {updated} sản phẩm.";

            return RedirectToAction("Index");
        }

        private string EscapeForCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            value = value.Replace("\"", "\"\"");

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n"))
            {
                return "\"" + value + "\"";
            }

            return value;
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (line == null)
                return result;

            bool inQuotes = false;
            var value = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        value.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(c);
                }
            }

            result.Add(value.ToString());
            return result;
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
