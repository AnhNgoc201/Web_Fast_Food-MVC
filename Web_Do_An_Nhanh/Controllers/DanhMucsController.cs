using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using Web_Do_An_Nhanh.Models;
using System.Data.Entity.Validation;

namespace Web_Do_An_Nhanh.Controllers
{
    public class DanhMucsController : Controller
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

        // BẮT BUỘC ĐĂNG NHẬP NHÂN VIÊN CHO TOÀN BỘ CONTROLLER
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!ChiChoNhanVien())
            {
                filterContext.Result = RedirectToAction("Login", "NhanVien");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
        // validate danh mục
        private void ValidateDanhMucInput(DanhMuc dm)
        {
            if (dm == null)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin trước khi lưu.");
                return;
            }

            dm.TenDM = dm.TenDM != null ? dm.TenDM.Trim() : null;
            dm.MoTa = dm.MoTa != null ? dm.MoTa.Trim() : null;

            if (string.IsNullOrWhiteSpace(dm.TenDM))
                ModelState.AddModelError("TenDM", "Vui lòng nhập tên danh mục.");

            if (string.IsNullOrWhiteSpace(dm.MoTa))
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

        public ActionResult Index(string searchString, int? page)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            var danhMucs = db.DanhMucs.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                danhMucs = danhMucs.Where(d =>
                    (d.TenDM != null && d.TenDM.Contains(searchString)) ||
                    (d.MoTa != null && d.MoTa.Contains(searchString)));
            }

            danhMucs = danhMucs.OrderBy(d => d.MaDM);

            int pageSize = 10;
            int pageNumber = page ?? 1;

            int totalItems = danhMucs.Count();
            var items = danhMucs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DanhMuc dm = db.DanhMucs.Find(id);
            if (dm == null)
                return HttpNotFound();

            return View(dm);
        }

        public ActionResult Create()
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaDM,TenDM,MoTa")] DanhMuc danhMuc)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            ValidateDanhMucInput(danhMuc);

            if (ModelState.IsValid)
            {
                try
                {
                    db.DanhMucs.Add(danhMuc);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm danh mục thành công.";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    AddEfValidationErrorsToModelState(ex);
                }
            }

            return View(danhMuc);
        }


        public ActionResult Edit(int? id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DanhMuc dm = db.DanhMucs.Find(id);
            if (dm == null)
                return HttpNotFound();

            return View(dm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaDM,TenDM,MoTa")] DanhMuc danhMuc)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            var existing = db.DanhMucs.Find(danhMuc.MaDM);
            if (existing == null)
                return HttpNotFound();

            ValidateDanhMucInput(danhMuc);

            if (!ModelState.IsValid)
                return View(danhMuc);

            existing.TenDM = danhMuc.TenDM;
            existing.MoTa = danhMuc.MoTa;

            try
            {
                db.SaveChanges();
                TempData["Success"] = "Cập nhật danh mục thành công.";
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException ex)
            {
                AddEfValidationErrorsToModelState(ex);
            }

            return View(danhMuc);
        }

        public ActionResult Delete(int? id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DanhMuc dm = db.DanhMucs.Find(id);
            if (dm == null)
                return HttpNotFound();

            return View(dm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            DanhMuc dm = db.DanhMucs.Find(id);
            if (dm == null)
                return HttpNotFound();

            try
            {
                db.DanhMucs.Remove(dm);
                db.SaveChanges();
                TempData["Success"] = "Xóa danh mục thành công.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa vì đang có sản phẩm thuộc danh mục này.";
            }

            return RedirectToAction("Index");
        }

        // import và export
        public ActionResult Export(string searchString)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            var query = db.DanhMucs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d =>
                    (d.TenDM != null && d.TenDM.Contains(searchString)) ||
                    (d.MoTa != null && d.MoTa.Contains(searchString)));
            }

            var list = query.OrderBy(d => d.MaDM).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("MaDM,TenDM,MoTa");

            foreach (var dm in list)
            {
                sb.AppendLine(string.Join(",",
                    dm.MaDM,
                    EscapeForCsv(dm.TenDM),
                    EscapeForCsv(dm.MoTa)
                ));
            }

            var encoding = new UTF8Encoding(true);
            byte[] buffer = encoding.GetBytes(sb.ToString());

            return File(buffer, "text/csv", "DanhMuc.csv");
        }

        public ActionResult Import()
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

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
                    if (cols.Count < 2)
                        continue;

                    int maDm;
                    int.TryParse(cols[0].Trim(), out maDm);
                    string tenDm = cols[1].Trim();
                    string moTa = cols.Count >= 3 ? cols[2].Trim() : null;

                    if (string.IsNullOrWhiteSpace(tenDm))
                        continue;

                    if (maDm > 0)
                    {
                        var existing = db.DanhMucs.Find(maDm);
                        if (existing != null)
                        {
                            existing.TenDM = tenDm;
                            existing.MoTa = moTa;
                            updated++;
                        }
                        else
                        {
                            var dm = new DanhMuc
                            {
                                TenDM = tenDm,
                                MoTa = moTa
                            };
                            db.DanhMucs.Add(dm);
                            inserted++;
                        }
                    }
                    else
                    {
                        var dm = new DanhMuc
                        {
                            TenDM = tenDm,
                            MoTa = moTa
                        };
                        db.DanhMucs.Add(dm);
                        inserted++;
                    }
                }
            }

            db.SaveChanges();

            TempData["ImportMessage"] = $"Import thành công. Thêm {inserted} danh mục mới, cập nhật {updated} danh mục.";
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
