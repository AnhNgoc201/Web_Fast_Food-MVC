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
    public class NhanVienController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        private NhanVien LayNhanVienDangNhap()
        {
            return Session["NhanVien"] as NhanVien;
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

        // validate nhân viên
        private void ValidateNhanVienInput(NhanVien nv, int? excludeMaNV = null)
        {
            if (nv == null)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin trước khi lưu.");
                return;
            }

            // trim cho sạch (giống khách hàng)
            nv.TenNV = nv.TenNV != null ? nv.TenNV.Trim() : null;
            nv.Email = nv.Email != null ? nv.Email.Trim() : null;
            nv.SDT = nv.SDT != null ? nv.SDT.Trim() : null;

            if (string.IsNullOrWhiteSpace(nv.TenNV))
                ModelState.AddModelError("TenNV", "Vui lòng nhập tên nhân viên.");

            if (string.IsNullOrWhiteSpace(nv.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email.");

            if (string.IsNullOrWhiteSpace(nv.SDT))
                ModelState.AddModelError("SDT", "Vui lòng nhập số điện thoại.");

            // chống trùng Email (giống khách hàng)
            if (!string.IsNullOrWhiteSpace(nv.Email))
            {
                var emailNorm = nv.Email.Trim().ToLower();

                bool emailDaTonTai = db.NhanViens.Any(x =>
                    x.Email != null &&
                    x.Email.Trim().ToLower() == emailNorm &&
                    (!excludeMaNV.HasValue || x.MaNV != excludeMaNV.Value));

                if (emailDaTonTai)
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
            }

            // chống trùng SDT (normalize như bạn đang làm để EF dịch được SQL)
            if (!string.IsNullOrWhiteSpace(nv.SDT))
            {
                var sdtNorm = nv.SDT.Trim()
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace(".", "")
                    .Replace("(", "")
                    .Replace(")", "");

                bool sdtDaTonTai = db.NhanViens.Any(x =>
                    x.SDT != null &&
                    x.SDT.Trim()
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Replace(".", "")
                        .Replace("(", "")
                        .Replace(")", "") == sdtNorm &&
                    (!excludeMaNV.HasValue || x.MaNV != excludeMaNV.Value));

                if (sdtDaTonTai)
                    ModelState.AddModelError("SDT", "Số điện thoại đã tồn tại trong hệ thống.");
            }

            if (!ModelState.IsValid)
                ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin.");
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

        //================================================
        // import và export
        public ActionResult Export(string searchString, int? roleId)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            var query = db.NhanViens.Include(n => n.VaiTro).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n =>
                    n.TenNV.Contains(searchString) ||
                    n.Email.Contains(searchString) ||
                    n.SDT.Contains(searchString));
            }

            if (roleId.HasValue && roleId.Value > 0)
            {
                query = query.Where(n => n.MaVT == roleId.Value);
            }

            var list = query.OrderBy(n => n.MaNV).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("MaNV,TenNV,MaVT,Email,SDT,MatKhau");

            foreach (var nv in list)
            {
                sb.AppendLine(string.Join(",",
                    nv.MaNV,
                    EscapeForCsv(nv.TenNV),
                    nv.MaVT,
                    EscapeForCsv(nv.Email),
                    EscapeForCsv(nv.SDT),
                    EscapeForCsv(nv.MatKhau)
                ));
            }

            var encoding = new UTF8Encoding(true);
            byte[] buffer = encoding.GetBytes(sb.ToString());

            return File(buffer, "text/csv", "NhanVien.csv");
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
                    if (cols.Count < 6)
                        continue;

                    int maNv;
                    int.TryParse(cols[0].Trim(), out maNv);
                    string tenNv = cols[1].Trim();
                    int maVt;
                    int.TryParse(cols[2].Trim(), out maVt);
                    string email = cols[3].Trim();
                    string sdt = cols[4].Trim();
                    string matKhau = cols[5].Trim();


                    if (maVt <= 0)
                        continue;

                    if (maNv > 0)
                    {
                        var existing = db.NhanViens.Find(maNv);
                        if (existing != null)
                        {
                            existing.TenNV = tenNv;
                            existing.MaVT = maVt;
                            existing.Email = email;
                            existing.SDT = sdt;
                            existing.MatKhau = matKhau;
                            updated++;
                        }
                        else
                        {
                            var nv = new NhanVien
                            {
                                TenNV = tenNv,
                                MaVT = maVt,
                                Email = email,
                                SDT = sdt,
                                MatKhau = matKhau
                            };
                            db.NhanViens.Add(nv);
                            inserted++;
                        }
                    }
                    else
                    {
                        var nv = new NhanVien
                        {
                            TenNV = tenNv,
                            MaVT = maVt,
                            Email = email,
                            SDT = sdt,
                            MatKhau = matKhau
                        };
                        db.NhanViens.Add(nv);
                        inserted++;
                    }
                }
            }

            db.SaveChanges();

            TempData["ImportMessage"] = $"Import thành công. Thêm {inserted} nhân viên mới, cập nhật {updated} nhân viên.";

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

        //================================================
        public ActionResult Index(string searchString, int? roleId, int? page)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            var nhanViens = db.NhanViens.Include(n => n.VaiTro).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                nhanViens = nhanViens.Where(n =>
                    (n.TenNV != null && n.TenNV.Contains(searchString)) ||
                    (n.Email != null && n.Email.Contains(searchString)) ||
                    (n.SDT != null && n.SDT.Contains(searchString)));
            }

            if (roleId.HasValue && roleId.Value > 0)
            {
                nhanViens = nhanViens.Where(n => n.MaVT == roleId.Value);
            }

            nhanViens = nhanViens.OrderBy(n => n.MaNV);
            int pageSize = 10;
            int pageNumber = page ?? 1;
            int totalItems = nhanViens.Count();
            var items = nhanViens
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentRoleId = roleId;
            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Roles = new SelectList(db.VaiTroes.ToList(), "MaVT", "TenVT");
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
                return HttpNotFound();

            return View(nhanVien);
        }

        public ActionResult Create()
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            ViewBag.MaVT = new SelectList(db.VaiTroes, "MaVT", "TenVT");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaNV,TenNV,MaVT,Email,SDT,MatKhau")] NhanVien nhanVien)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            ValidateNhanVienInput(nhanVien);

            if (ModelState.IsValid)
            {
                try
                {
                    db.NhanViens.Add(nhanVien);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm nhân viên thành công!";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    AddEfValidationErrorsToModelState(ex);
                }
            }

            ViewBag.MaVT = new SelectList(db.VaiTroes, "MaVT", "TenVT", nhanVien.MaVT);
            return View(nhanVien);
        }

        public ActionResult Edit(int? id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
                return HttpNotFound();

            ViewBag.MaVT = new SelectList(db.VaiTroes, "MaVT", "TenVT", nhanVien.MaVT);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaNV,TenNV,MaVT,Email,SDT,MatKhau")] NhanVien nhanVien)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            var existing = db.NhanViens.Find(nhanVien.MaNV);
            if (existing == null)
                return HttpNotFound();

            ValidateNhanVienInput(nhanVien, nhanVien.MaNV);

            if (!ModelState.IsValid)
            {
                ViewBag.MaVT = new SelectList(db.VaiTroes, "MaVT", "TenVT", nhanVien.MaVT);
                return View(nhanVien);
            }

            bool dangKhoa = existing.MatKhau != null && existing.MatKhau.StartsWith("KHOA_");
            existing.TenNV = nhanVien.TenNV;
            existing.MaVT = nhanVien.MaVT;
            existing.Email = nhanVien.Email;
            existing.SDT = nhanVien.SDT;

            if (!string.IsNullOrWhiteSpace(nhanVien.MatKhau))
            {
                if (dangKhoa)
                {
                    ModelState.AddModelError("MatKhau", "Tài khoản đang bị khóa. Hãy dùng chức năng Mở khóa để đặt lại mật khẩu.");
                    ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin.");

                    ViewBag.MaVT = new SelectList(db.VaiTroes, "MaVT", "TenVT", nhanVien.MaVT);
                    return View(nhanVien);
                }

                existing.MatKhau = nhanVien.MatKhau.Trim();
            }

            try
            {
                db.SaveChanges();
                TempData["Success"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException ex)
            {
                AddEfValidationErrorsToModelState(ex);
            }

            ViewBag.MaVT = new SelectList(db.VaiTroes, "MaVT", "TenVT", nhanVien.MaVT);
            return View(nhanVien);
        }

        public ActionResult Delete(int? id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
                return HttpNotFound();

            return View(nhanVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NhanVien nvDangNhap = Session["NhanVien"] as NhanVien;
            if (nvDangNhap != null && nvDangNhap.MaNV == id)
            {
                TempData["Error"] = "Không thể khóa tài khoản đang đăng nhập và tài khoản là Admin.";
                return RedirectToAction("Index");
            }
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
                return HttpNotFound();

            if (nhanVien.MaVT == 1)
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin.";
                return RedirectToAction("Index");
            }

            bool coHoaDon = db.HoaDons.Any(h => h.MaNV == id);
            if (coHoaDon)
            {
                TempData["Error"] = "Nhân viên này đã phát sinh dữ liệu, không thể xóa. " +
                                    "Bạn hãy dùng chức năng khóa nhân viên.";
                return RedirectToAction("Khoa", new { id = id });
            }

            db.NhanViens.Remove(nhanVien);
            db.SaveChanges();
            TempData["Success"] = "Xóa nhân viên thành công!";
            return RedirectToAction("Index");
        }


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            string email = form["Email"];
            string pass = form["Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Email và Mật khẩu.";
                return View();
            }

            NhanVien nv = db.NhanViens.FirstOrDefault(x => x.Email == email && x.MatKhau == pass);

            if (nv != null)
            {
                if (nv.MatKhau != null && nv.MatKhau.StartsWith("KHOA_"))
                {
                    ViewBag.Error = "Tài khoản nhân viên đã bị khóa.";
                    return View();
                }

                Session["NhanVien"] = nv;
                TempData["Message"] = "Đăng nhập nhân viên thành công!";
                return RedirectToAction("Index", "Admin");
            }


            ViewBag.Error = "Sai thông tin đăng nhập nhân viên.";
            return View();
        }

        public ActionResult Logout()
        {
            Session["NhanVien"] = null;
            TempData["Message"] = "Bạn đã đăng xuất khỏi hệ thống nhân viên.";
            return RedirectToAction("Login", "NhanVien");
        }

        public ActionResult Khoa(int? id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
                return HttpNotFound();

            return View(nhanVien);
        }

        [HttpPost, ActionName("Khoa")]
        [ValidateAntiForgeryToken]
        public ActionResult KhoaConfirmed(int id)
        {
            NhanVien nvDangNhap = Session["NhanVien"] as NhanVien;
            if (nvDangNhap != null && nvDangNhap.MaNV == id)
            {
                TempData["Error"] = "Không thể khóa tài khoản đang đăng nhập và tài khoản là Admin.";
                return RedirectToAction("Index");
            }


            if (!ChiChoAdmin())
                return KhongCoQuyen();

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
                return HttpNotFound();

            if (nhanVien.MaVT == 1)
            {
                TempData["Error"] = "Không thể khóa tài khoản Admin.";
                return RedirectToAction("Index");
            }

            if (nhanVien.MatKhau != null && nhanVien.MatKhau.StartsWith("KHOA_"))
            {
                TempData["Error"] = "Nhân viên này đã bị khóa trước đó.";
                return RedirectToAction("Index");
            }

            nhanVien.MatKhau = "KHOA_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            db.Entry(nhanVien).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "Khóa nhân viên thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoKhoa(int id)
        {
            if (!ChiChoAdmin())
                return KhongCoQuyen();

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
                return HttpNotFound();

            if (nhanVien.MatKhau == null || !nhanVien.MatKhau.StartsWith("KHOA_"))
            {
                TempData["Error"] = "Nhân viên này hiện không bị khóa.";
                return RedirectToAction("Index");
            }

            nhanVien.MatKhau = "123456";
            db.Entry(nhanVien).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "Mở khóa nhân viên thành công!";
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
