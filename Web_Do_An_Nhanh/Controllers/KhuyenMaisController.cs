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
    public class KhuyenMaisController : Controller
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

        // validate khuyến mãi
        private void ValidateKhuyenMaiInput(KhuyenMai km)
        {
            if (km == null)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin trước khi lưu.");
                return;
            }

            km.TenKM = km.TenKM != null ? km.TenKM.Trim() : null;
            km.MoTa = km.MoTa != null ? km.MoTa.Trim() : null;

            if (string.IsNullOrWhiteSpace(km.TenKM))
                ModelState.AddModelError("TenKM", "Vui lòng nhập tên khuyến mãi.");

            // nếu bạn muốn MoTa bắt buộc giống “không bỏ trống”
            if (string.IsNullOrWhiteSpace(km.MoTa))
                ModelState.AddModelError("MoTa", "Vui lòng nhập mô tả.");

            // phần trăm giảm hợp lệ (thường 1..100)
            if (!km.PhanTramGiam.HasValue)
                ModelState.AddModelError("PhanTramGiam", "Vui lòng nhập phần trăm giảm.");
            else if (km.PhanTramGiam.Value <= 0 || km.PhanTramGiam.Value > 100)
                ModelState.AddModelError("PhanTramGiam", "Phần trăm giảm phải từ 1 đến 100.");

            if (!km.NgayBatDau.HasValue)
                ModelState.AddModelError("NgayBatDau", "Vui lòng chọn ngày bắt đầu.");

            if (!km.NgayKetThuc.HasValue)
                ModelState.AddModelError("NgayKetThuc", "Vui lòng chọn ngày kết thúc.");

            if (km.NgayBatDau.HasValue && km.NgayKetThuc.HasValue && km.NgayBatDau.Value > km.NgayKetThuc.Value)
                ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

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
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var kmQuery = db.KhuyenMais.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                kmQuery = kmQuery.Where(k =>
                    (k.TenKM != null && k.TenKM.Contains(searchString)) ||
                    (k.MoTa != null && k.MoTa.Contains(searchString)));
            }

            kmQuery = kmQuery.OrderByDescending(k => k.NgayBatDau);

            int pageSize = 10;
            int pageNumber = page ?? 1;

            int totalItems = kmQuery.Count();
            var items = kmQuery
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
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KhuyenMai km = db.KhuyenMais.Find(id);
            if (km == null)
                return HttpNotFound();

            return View(km);
        }

        public ActionResult Create()
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaKM,TenKM,MoTa,PhanTramGiam,NgayBatDau,NgayKetThuc")] KhuyenMai khuyenMai)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            ValidateKhuyenMaiInput(khuyenMai);

            if (ModelState.IsValid)
            {
                try
                {
                    db.KhuyenMais.Add(khuyenMai);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm khuyến mãi thành công.";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    AddEfValidationErrorsToModelState(ex);
                }
            }

            return View(khuyenMai);
        }

        public ActionResult Edit(int? id)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KhuyenMai km = db.KhuyenMais.Find(id);
            if (km == null)
                return HttpNotFound();

            return View(km);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaKM,TenKM,MoTa,PhanTramGiam,NgayBatDau,NgayKetThuc")] KhuyenMai khuyenMai)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var existing = db.KhuyenMais.Find(khuyenMai.MaKM);
            if (existing == null)
                return HttpNotFound();

            ValidateKhuyenMaiInput(khuyenMai);

            if (!ModelState.IsValid)
                return View(khuyenMai);

            existing.TenKM = khuyenMai.TenKM;
            existing.MoTa = khuyenMai.MoTa;
            existing.PhanTramGiam = khuyenMai.PhanTramGiam;
            existing.NgayBatDau = khuyenMai.NgayBatDau;
            existing.NgayKetThuc = khuyenMai.NgayKetThuc;

            try
            {
                db.SaveChanges();
                TempData["Success"] = "Cập nhật khuyến mãi thành công.";
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException ex)
            {
                AddEfValidationErrorsToModelState(ex);
            }

            return View(khuyenMai);
        }


        public ActionResult Delete(int? id)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KhuyenMai km = db.KhuyenMais.Find(id);
            if (km == null)
                return HttpNotFound();

            return View(km);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            KhuyenMai km = db.KhuyenMais.Find(id);
            if (km != null)
            {
                db.KhuyenMais.Remove(km);
                db.SaveChanges();
                TempData["Success"] = "Xóa khuyến mãi thành công.";
            }

            return RedirectToAction("Index");
        }

        // ========== EXPORT ==========
        public ActionResult Export(string searchString)
        {
            if (!ChiChoAdmin()) return KhongCoQuyen();

            var query = db.KhuyenMais.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(k =>
                    (k.TenKM != null && k.TenKM.Contains(searchString)) ||
                    (k.MoTa != null && k.MoTa.Contains(searchString)));
            }

            var list = query
                .OrderByDescending(k => k.NgayBatDau)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("MaKM,TenKM,MoTa,PhanTramGiam,NgayBatDau,NgayKetThuc");

            foreach (var km in list)
            {
                sb.AppendLine(string.Join(",",
                    km.MaKM,
                    EscapeForCsv(km.TenKM),
                    EscapeForCsv(km.MoTa),
                    km.PhanTramGiam,
                    //km.NgayBatDau.Value.ToString("dd/MM/yyyy"),
                    //km.NgayKetThuc.Value.ToString("dd/MM/yyyy")
                    km.NgayBatDau.HasValue ? km.NgayBatDau.Value.ToString("dd/MM/yyyy") : "",
                    km.NgayKetThuc.HasValue ? km.NgayKetThuc.Value.ToString("dd/MM/yyyy") : ""

                ));
            }

            var encoding = new UTF8Encoding(true);
            byte[] buffer = encoding.GetBytes(sb.ToString());

            return File(buffer, "text/csv", "KhuyenMai.csv");
        }

        // ========== IMPORT ==========

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

                    int maKm;
                    int.TryParse(cols[0].Trim(), out maKm);

                    string tenKm = cols[1].Trim();
                    string moTa = cols[2].Trim();

                    int phanTram;
                    int.TryParse(cols[3].Trim(), out phanTram);

                    DateTime ngayBatDau;
                    DateTime ngayKetThuc;

                    if (string.IsNullOrWhiteSpace(tenKm))
                        continue;

                    if (!DateTime.TryParse(cols[4].Trim(), out ngayBatDau))
                        continue;

                    if (!DateTime.TryParse(cols[5].Trim(), out ngayKetThuc))
                        continue;

                    if (ngayBatDau > ngayKetThuc)
                        continue;

                    if (maKm > 0)
                    {
                        var existing = db.KhuyenMais.Find(maKm);
                        if (existing != null)
                        {
                            existing.TenKM = tenKm;
                            existing.MoTa = moTa;
                            existing.PhanTramGiam = phanTram;
                            existing.NgayBatDau = ngayBatDau;
                            existing.NgayKetThuc = ngayKetThuc;
                            updated++;
                        }
                        else
                        {
                            var km = new KhuyenMai
                            {
                                TenKM = tenKm,
                                MoTa = moTa,
                                PhanTramGiam = phanTram,
                                NgayBatDau = ngayBatDau,
                                NgayKetThuc = ngayKetThuc
                            };
                            db.KhuyenMais.Add(km);
                            inserted++;
                        }
                    }
                    else
                    {
                        var km = new KhuyenMai
                        {
                            TenKM = tenKm,
                            MoTa = moTa,
                            PhanTramGiam = phanTram,
                            NgayBatDau = ngayBatDau,
                            NgayKetThuc = ngayKetThuc
                        };
                        db.KhuyenMais.Add(km);
                        inserted++;
                    }
                }
            }

            db.SaveChanges();

            TempData["ImportMessage"] =
                $"Import thành công. Thêm {inserted} khuyến mãi mới, cập nhật {updated} khuyến mãi.";

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
