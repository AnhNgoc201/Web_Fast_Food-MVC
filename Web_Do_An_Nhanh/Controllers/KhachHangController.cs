using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;
using System.IO;
using System.Text;
using System.Data.Entity.Validation;

namespace Web_Do_An_Nhanh.Controllers
{
    public class KhachHangController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        //yêu cầu đăng nhập nhân viên
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

        // validate khách hàng
        // validate khách hàng
        private void ValidateKhachHangInput(KhachHang kh, int? excludeMaKh = null)
        {
            if (kh == null)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin trước khi lưu.");
                return;
            }

            // trim cho sạch
            kh.TenKH = kh.TenKH != null ? kh.TenKH.Trim() : null;
            kh.Email = kh.Email != null ? kh.Email.Trim() : null;
            kh.SDT = kh.SDT != null ? kh.SDT.Trim() : null;
            kh.DiaChi = kh.DiaChi != null ? kh.DiaChi.Trim() : null;

            if (string.IsNullOrWhiteSpace(kh.TenKH))
                ModelState.AddModelError("TenKH", "Vui lòng nhập tên khách hàng.");

            if (string.IsNullOrWhiteSpace(kh.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email.");

            if (string.IsNullOrWhiteSpace(kh.SDT))
                ModelState.AddModelError("SDT", "Vui lòng nhập số điện thoại.");

            if (string.IsNullOrWhiteSpace(kh.DiaChi))
                ModelState.AddModelError("DiaChi", "Vui lòng nhập địa chỉ.");

            // chống trùng Email / SĐT (server-side)
            if (!string.IsNullOrWhiteSpace(kh.Email))
            {
                var emailNorm = kh.Email.Trim().ToLower();
                bool emailDaTonTai = db.KhachHangs.Any(x =>
                    x.Email != null &&
                    x.Email.Trim().ToLower() == emailNorm &&
                    (!excludeMaKh.HasValue || x.MaKH != excludeMaKh.Value));

                if (emailDaTonTai)
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
            }

            if (!string.IsNullOrWhiteSpace(kh.SDT))
            {
                var sdtNorm = kh.SDT.Trim()
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace(".", "")
                    .Replace("(", "")
                    .Replace(")", "");

                bool sdtDaTonTai = db.KhachHangs.Any(x =>
                    x.SDT != null &&
                    x.SDT.Trim()
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Replace(".", "")
                        .Replace("(", "")
                        .Replace(")", "") == sdtNorm &&
                    (!excludeMaKh.HasValue || x.MaKH != excludeMaKh.Value));

                if (sdtDaTonTai)
                    ModelState.AddModelError("SDT", "Số điện thoại đã tồn tại trong hệ thống.");
            }

            if (!ModelState.IsValid)
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin trước khi lưu.");
        }

        private void AddEfValidationErrorsToModelState(DbEntityValidationException ex)
        {
            foreach (var eve in ex.EntityValidationErrors)
            {
                foreach (var ve in eve.ValidationErrors)
                {
                    // ve.PropertyName có thể rỗng, nhưng AddModelError vẫn ok
                    ModelState.AddModelError(ve.PropertyName, ve.ErrorMessage);
                }
            }

            // message chung để hiện ở ValidationSummary
            ModelState.AddModelError("", "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.");
        }


        public ActionResult Index(string searchString, int? page)
        {
            var khachHangs = db.KhachHangs.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                khachHangs = khachHangs.Where(k =>
                    (k.TenKH != null && k.TenKH.Contains(searchString)) ||
                    (k.Email != null && k.Email.Contains(searchString)) ||
                    (k.SDT != null && k.SDT.Contains(searchString)) ||
                    (k.DiaChi != null && k.DiaChi.Contains(searchString)));
            }
            khachHangs = khachHangs.OrderBy(k => k.MaKH);
            int pageSize = 10;
            int pageNumber = page ?? 1;
            int totalItems = khachHangs.Count();
            var items = khachHangs
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
        // import và export
        public ActionResult Export(string searchString)
        {
            var query = db.KhachHangs.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(k =>
                    k.TenKH.Contains(searchString) ||
                    k.Email.Contains(searchString) ||
                    k.SDT.Contains(searchString) ||
                    k.DiaChi.Contains(searchString));
            }
            var list = query.OrderBy(k => k.MaKH).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("MaKH,TenKH,Email,SDT,DiaChi");
            foreach (var kh in list)
            {
                sb.AppendLine(string.Join(",",
                    kh.MaKH,
                    EscapeForCsv(kh.TenKH),
                    EscapeForCsv(kh.Email),
                    EscapeForCsv(kh.SDT),
                    EscapeForCsv(kh.DiaChi)
                ));
            }
            var encoding = new UTF8Encoding(true);
            byte[] buffer = encoding.GetBytes(sb.ToString());

            return File(buffer, "text/csv", "KhachHang.csv");
            //var encoding = Encoding.Default; 

            //byte[] buffer = encoding.GetBytes(sb.ToString());
            //return File(buffer, "text/csv", "KhachHang.csv");

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

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase file)
        {
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
                    if (cols.Count < 5)
                        continue;

                    int maKh;
                    int.TryParse(cols[0].Trim(), out maKh);
                    string tenKh = cols[1].Trim();
                    string email = cols[2].Trim();
                    string sdt = cols[3].Trim();
                    string diaChi = cols[4].Trim();

                    if (maKh > 0)
                    {
                        var existing = db.KhachHangs.Find(maKh);
                        if (existing != null)
                        {
                            existing.TenKH = tenKh;
                            existing.Email = email;
                            existing.SDT = sdt;
                            existing.DiaChi = diaChi;
                            updated++;
                        }
                        else
                        {
                            var khNew = new KhachHang
                            {
                                TenKH = tenKh,
                                Email = email,
                                SDT = sdt,
                                DiaChi = diaChi,
                                MatKhau = "123456"
                            };
                            db.KhachHangs.Add(khNew);
                            inserted++;
                        }
                    }
                    else
                    {
                        var khNew = new KhachHang
                        {
                            TenKH = tenKh,
                            Email = email,
                            SDT = sdt,
                            DiaChi = diaChi,
                            MatKhau = "123456"
                        };
                        db.KhachHangs.Add(khNew);
                        inserted++;
                    }
                }
            }

            db.SaveChanges();

            TempData["ImportMessage"] = $"Import thành công. Thêm {inserted} khách hàng mới, cập nhật {updated} khách hàng.";

            return RedirectToAction("Index");
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
        //==========================================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
                return HttpNotFound();

            return View(khachHang);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaKH,TenKH,Email,SDT,DiaChi,MatKhau")] KhachHang khachHang)
        {
            ValidateKhachHangInput(khachHang);

            if (string.IsNullOrWhiteSpace(khachHang.MatKhau))
                khachHang.MatKhau = "123456";

            if (ModelState.IsValid)
            {
                try
                {
                    db.KhachHangs.Add(khachHang);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    AddEfValidationErrorsToModelState(ex);
                }
            }

            return View(khachHang);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
                return HttpNotFound();

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaKH,TenKH,Email,SDT,DiaChi")] KhachHang khachHang)
        {
            ValidateKhachHangInput(khachHang, khachHang.MaKH);

            if (ModelState.IsValid)
            {
                var existing = db.KhachHangs.Find(khachHang.MaKH);
                if (existing == null)
                    return HttpNotFound();

                // chỉ update các field cho phép
                existing.TenKH = khachHang.TenKH;
                existing.Email = khachHang.Email;
                existing.SDT = khachHang.SDT;
                existing.DiaChi = khachHang.DiaChi;

                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    AddEfValidationErrorsToModelState(ex);
                }
            }

            return View(khachHang);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
                return HttpNotFound();

            return View(khachHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
                return HttpNotFound();

            bool coHoaDon = db.HoaDons.Any(h => h.MaKH == id);
            if (coHoaDon)
            {
                TempData["Error"] = "Khách hàng này đang có hóa đơn, không thể xóa. " +
                                    "Bạn hãy xóa hóa đơn liên quan trước hoặc dùng chức năng khóa khách hàng.";
                return RedirectToAction("Khoa", new { id = id });
            }

            db.KhachHangs.Remove(khachHang);
            db.SaveChanges();
            TempData["Success"] = "Xóa khách hàng thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult Khoa(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
                return HttpNotFound();

            return View(khachHang);
        }

        [HttpPost, ActionName("Khoa")]
        [ValidateAntiForgeryToken]
        public ActionResult KhoaConfirmed(int id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
                return HttpNotFound();

            if (khachHang.MatKhau != null && khachHang.MatKhau.StartsWith("KHOA_"))
            {
                TempData["Error"] = "Khách hàng này đã bị khóa trước đó.";
                return RedirectToAction("Index");
            }

            khachHang.MatKhau = "KHOA_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            if (khachHang.TenKH != null && !khachHang.TenKH.StartsWith("[DA KHOA] "))
                khachHang.TenKH = "[DA KHOA] " + khachHang.TenKH;

            db.Entry(khachHang).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "Khóa khách hàng thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoKhoa(int id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
                return HttpNotFound();

            if (khachHang.MatKhau == null || !khachHang.MatKhau.StartsWith("KHOA_"))
            {
                TempData["Error"] = "Khách hàng này hiện không bị khóa.";
                return RedirectToAction("Index");
            }

            khachHang.MatKhau = "123456";

            if (khachHang.TenKH != null && khachHang.TenKH.StartsWith("[DA KHOA] "))
                khachHang.TenKH = khachHang.TenKH.Substring(10);

            db.Entry(khachHang).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "Mở khóa khách hàng thành công!";
            return RedirectToAction("Index");
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