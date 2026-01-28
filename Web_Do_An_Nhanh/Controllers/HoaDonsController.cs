using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.Text;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class HoaDonsController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        // ====== HỖ TRỢ ĐĂNG NHẬP & PHÂN QUYỀN ======

        private NhanVien LayNhanVienDangNhap()
        {
            return Session["NhanVien"] as NhanVien;
        }

        private bool ChiChoNhanVien()
        {
            var nv = LayNhanVienDangNhap();
            return nv != null && (nv.MaVT == 1 || nv.MaVT == 2);
        }

        private bool LaAdmin()
        {
            var nv = LayNhanVienDangNhap();
            return nv != null && nv.MaVT == 1;
        }

        private ActionResult KhongCoQuyen()
        {
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

        // ====== HÀM DÙNG CHUNG: ÁP DỤNG FILTER ======

        private IQueryable<HoaDon> ApplyFilters(
            string searchString,
            int? maKH,
            int? maNV,
            int? maTT,
            DateTime? fromDate,
            DateTime? toDate,
            decimal? minTotal,
            decimal? maxTotal)
        {
            var query = db.HoaDons
                          .Include(h => h.KhachHang)
                          .Include(h => h.NhanVien)
                          .Include(h => h.KhuyenMai)
                          .Include(h => h.TinhTrang)
                          .Include(h => h.ChiTietHoaDons.Select(ct => ct.SanPham))
                          .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string keyword = searchString.Trim().ToLower();
                query = query.Where(h =>
                    h.MaHD.ToString().Contains(keyword) ||
                    (h.KhachHang != null && h.KhachHang.TenKH.ToLower().Contains(keyword)) ||
                    (h.NhanVien != null && h.NhanVien.TenNV.ToLower().Contains(keyword)) ||
                    (h.DiaChi != null && h.DiaChi.ToLower().Contains(keyword)));
            }


            if (maKH.HasValue)
                query = query.Where(h => h.MaKH == maKH.Value);

            if (maNV.HasValue)
                query = query.Where(h => h.MaNV == maNV.Value);

            if (maTT.HasValue)
                query = query.Where(h => h.MaTT == maTT.Value);

            if (fromDate.HasValue)
                query = query.Where(h => h.NgayLap >= fromDate.Value);

            if (toDate.HasValue)
            {
                DateTime to = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(h => h.NgayLap <= to);
            }

            if (minTotal.HasValue)
                query = query.Where(h => h.TongTien >= minTotal.Value);

            if (maxTotal.HasValue)
                query = query.Where(h => h.TongTien <= maxTotal.Value);

            return query;
        }

        // ====== DANH SÁCH HÓA ĐƠN (LỌC + SẮP XẾP + PHÂN TRANG) ======

        public ActionResult Index(
            string searchString,
            int? maKH,
            int? maNV,
            int? maTT,
            DateTime? fromDate,
            DateTime? toDate,
            decimal? minTotal,
            decimal? maxTotal,
            string sortOrder,
            int page = 1,
            int pageSize = 10)
        {
            bool laAdmin = LaAdmin();
            ViewBag.LaAdmin = laAdmin;

            var nvDangNhap = LayNhanVienDangNhap();
            ViewBag.TenNhanVienDangNhap = nvDangNhap != null ? nvDangNhap.TenNV : "";

            // ====== NHÂN VIÊN CHỈ THẤY HÓA ĐƠN CỦA MÌNH ======
            if (!laAdmin && nvDangNhap != null)
            {
                maNV = nvDangNhap.MaNV;
            }

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentMaKH = maKH;
            ViewBag.CurrentMaNV = maNV;
            ViewBag.CurrentMaTT = maTT;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.MinTotal = minTotal;
            ViewBag.MaxTotal = maxTotal;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewBag.TotalSort = sortOrder == "total_asc" ? "total_desc" : "total_asc";

            var query = ApplyFilters(searchString, maKH, maNV, maTT, fromDate, toDate, minTotal, maxTotal);

            // Sắp xếp
            switch (sortOrder)
            {
                case "date_asc":
                    query = query.OrderBy(h => h.NgayLap);
                    break;
                case "total_asc":
                    query = query.OrderBy(h => h.TongTien);
                    break;
                case "total_desc":
                    query = query.OrderByDescending(h => h.TongTien);
                    break;
                default:
                    query = query.OrderByDescending(h => h.NgayLap);
                    break;
            }

            // Phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var hoaDons = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            ViewBag.KhachHangList = new SelectList(db.KhachHangs, "MaKH", "TenKH", maKH);

            // ====== NHÂN VIÊN LIST: nếu không phải admin thì chỉ load đúng 1 NV đang đăng nhập ======
            if (!laAdmin && nvDangNhap != null)
                ViewBag.NhanVienList = new SelectList(db.NhanViens.Where(n => n.MaNV == nvDangNhap.MaNV), "MaNV", "TenNV", maNV);
            else
                ViewBag.NhanVienList = new SelectList(db.NhanViens, "MaNV", "TenNV", maNV);

            ViewBag.TinhTrangList = new SelectList(db.TinhTrangs, "MaTT", "TenTT", maTT);
            ViewBag.TinhTrangAll = new SelectList(db.TinhTrangs, "MaTT", "TenTT");

            return View(hoaDons);
        }


        // ====== BULK UPDATE TRẠNG THÁI ======

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkUpdateStatus(
        int[] selectedIds,
        int maTTMoi,
        string searchString,
        int? maKH,
        int? maNV,
        int? maTT,
        DateTime? fromDate,
        DateTime? toDate,
        decimal? minTotal,
        decimal? maxTotal,
        string sortOrder,
        int page = 1,
        int pageSize = 10)
        {
            bool laAdmin = LaAdmin();
            var nvDangNhap = LayNhanVienDangNhap();

            // Nhân viên: ép filter maNV về chính mình để redirect về đúng list
            if (!laAdmin && nvDangNhap != null)
            {
                maNV = nvDangNhap.MaNV;
            }

            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 hóa đơn để cập nhật.";
                return RedirectToAction("Index", new
                {
                    searchString,
                    maKH,
                    maNV,
                    maTT,
                    fromDate,
                    toDate,
                    minTotal,
                    maxTotal,
                    sortOrder,
                    page,
                    pageSize
                });
            }

            var q = db.HoaDons.Where(h => selectedIds.Contains(h.MaHD));

            // ====== NHÂN VIÊN CHỈ ĐƯỢC UPDATE HÓA ĐƠN CỦA MÌNH ======
            if (!laAdmin && nvDangNhap != null)
            {
                q = q.Where(h => h.MaNV == nvDangNhap.MaNV);
            }

            var hoaDons = q.ToList();

            if (hoaDons.Count == 0)
            {
                TempData["Error"] = "Không có hóa đơn hợp lệ để cập nhật.";
                return RedirectToAction("Index", new
                {
                    searchString,
                    maKH,
                    maNV,
                    maTT,
                    fromDate,
                    toDate,
                    minTotal,
                    maxTotal,
                    sortOrder,
                    page,
                    pageSize
                });
            }

            foreach (var hd in hoaDons)
            {
                hd.MaTT = maTTMoi;
            }

            db.SaveChanges();

            int boQua = selectedIds.Length - hoaDons.Count;
            if (boQua > 0)
                TempData["Success"] = $"Đã cập nhật trạng thái cho {hoaDons.Count} hóa đơn. Bỏ qua {boQua} hóa đơn không thuộc quyền của bạn.";
            else
                TempData["Success"] = $"Đã cập nhật trạng thái cho {hoaDons.Count} hóa đơn.";

            return RedirectToAction("Index", new
            {
                searchString,
                maKH,
                maNV,
                maTT,
                fromDate,
                toDate,
                minTotal,
                maxTotal,
                sortOrder,
                page,
                pageSize
            });
        }


        // ====== EXPORT EXCEL (CHỈ ADMIN) ======
        public ActionResult Export(
            string searchString,
            int? maKH,
            int? maNV,
            int? maTT,
            DateTime? fromDate,
            DateTime? toDate,
            decimal? minTotal,
            decimal? maxTotal,
            string sortOrder)
        {
            if (!LaAdmin()) return KhongCoQuyen();

            var query = ApplyFilters(searchString, maKH, maNV, maTT, fromDate, toDate, minTotal, maxTotal);

            switch (sortOrder)
            {
                case "date_asc":
                    query = query.OrderBy(h => h.NgayLap);
                    break;
                case "total_asc":
                    query = query.OrderBy(h => h.TongTien);
                    break;
                case "total_desc":
                    query = query.OrderByDescending(h => h.TongTien);
                    break;
                default:
                    query = query.OrderByDescending(h => h.NgayLap);
                    break;
            }

            var list = query.ToList();

            var sb = new StringBuilder();
            sb.AppendLine("MaHD,MaKH,MaNV,MaTT,NgayLap,TongTien,PhiShip,DiaChi");

            foreach (var hd in list)
            {
                sb.AppendLine(string.Join(",",
                    hd.MaHD,
                    hd.MaKH,
                    hd.MaNV,
                    hd.MaTT,
                    EscapeForCsv(hd.NgayLap.HasValue ? hd.NgayLap.Value.ToString("dd/MM/yyyy HH:mm") : ""),
                    hd.TongTien,
                    hd.PhiShip,
                    EscapeForCsv(hd.DiaChi)
                ));
            }

            var encoding = new UTF8Encoding(true);
            byte[] buffer = encoding.GetBytes(sb.ToString());
            return File(buffer, "text/csv", "HoaDon.csv");
        }
        //=========
        public ActionResult Import()
        {
            if (!LaAdmin()) return KhongCoQuyen();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (!LaAdmin()) return KhongCoQuyen();

            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file CSV để import.");
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
                    if (cols.Count < 8)
                        continue;

                    int maHD;
                    int.TryParse(cols[0], out maHD);

                    int maKH;
                    int.TryParse(cols[1], out maKH);

                    int maNV;
                    int.TryParse(cols[2], out maNV);

                    int maTT;
                    int.TryParse(cols[3], out maTT);

                    DateTime ngayLap;
                    DateTime.TryParse(cols[4], out ngayLap);

                    decimal tongTien;
                    decimal.TryParse(cols[5], out tongTien);

                    bool phiShip;
                    bool.TryParse(cols[6], out phiShip);

                    string diaChi = cols[7];

                    HoaDon hd = null;

                    if (maHD > 0)
                        hd = db.HoaDons.Find(maHD);

                    bool isNew = false;
                    if (hd == null)
                    {
                        hd = new HoaDon();
                        isNew = true;
                    }

                    if (maKH > 0) hd.MaKH = maKH;
                    if (maNV > 0) hd.MaNV = maNV;
                    if (maTT > 0) hd.MaTT = maTT;

                    if (ngayLap != default(DateTime))
                        hd.NgayLap = ngayLap;
                    else if (isNew)
                        hd.NgayLap = DateTime.Now;

                    if (tongTien > 0)
                        hd.TongTien = tongTien;

                    hd.PhiShip = phiShip;
                    hd.DiaChi = diaChi;

                    if (isNew)
                    {
                        db.HoaDons.Add(hd);
                        inserted++;
                    }
                    else
                    {
                        db.Entry(hd).State = EntityState.Modified;
                        updated++;
                    }
                }
            }

            db.SaveChanges();

            TempData["ImportMessage"] = $"Import thành công! Thêm {inserted} hóa đơn, cập nhật {updated} hóa đơn.";
            return RedirectToAction("Index");
        }
        //=========
        private string EscapeForCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            value = value.Replace("\"", "\"\"");
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value + "\"";

            return value;
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (line == null) return result;

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
        // ====== CHI TIẾT HÓA ĐƠN ======

        public ActionResult Details(int? id)
        {

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var hoaDon = db.HoaDons
                           .Include(h => h.KhachHang)
                           .Include(h => h.NhanVien)
                           .Include(h => h.KhuyenMai)
                           .Include(h => h.TinhTrang)
                           .Include(h => h.ChiTietHoaDons.Select(ct => ct.SanPham))
                           .FirstOrDefault(h => h.MaHD == id);

            if (hoaDon == null)
                return HttpNotFound();

            if (!LaAdmin())
            {
                var nv = LayNhanVienDangNhap();
                if (nv == null || !hoaDon.MaNV.HasValue || hoaDon.MaNV.Value != nv.MaNV)
                    return KhongCoQuyen();
            }

            ViewBag.LaAdmin = LaAdmin();
            ViewBag.LichSu = new[]
            {
                new {
                    ThoiGian = hoaDon.NgayLap,
                    NhanVien = hoaDon.NhanVien != null ? hoaDon.NhanVien.TenNV : "Hệ thống",
                    TrangThai = hoaDon.TinhTrang != null ? hoaDon.TinhTrang.TenTT : ""
                }
            };

            return View(hoaDon);
        }

        // ====== TẠO HÓA ĐƠN ======
        public ActionResult Create()
        {
            if (!ChiChoNhanVien())
                return KhongCoQuyen();

            ViewBag.LaAdmin = LaAdmin();

            var nvDangNhap = LayNhanVienDangNhap();
            ViewBag.TenNhanVienDangNhap = nvDangNhap != null ? nvDangNhap.TenNV : "";

            ViewBag.MaKH = new SelectList(db.KhachHangs, "MaKH", "TenKH");

            if (LaAdmin())
                ViewBag.MaNV = new SelectList(db.NhanViens, "MaNV", "TenNV");
            else
                ViewBag.MaNV = new SelectList(db.NhanViens.Where(n => n.MaNV == nvDangNhap.MaNV), "MaNV", "TenNV", nvDangNhap.MaNV);

            ViewBag.MaKM = new SelectList(db.KhuyenMais, "MaKM", "TenKM");

            var daGiao = db.TinhTrangs.FirstOrDefault(t => t.TenTT == "Đã giao");
            ViewBag.MaTT = new SelectList(db.TinhTrangs, "MaTT", "TenTT", daGiao != null ? (int?)daGiao.MaTT : null);

            ViewBag.SanPhams = db.SanPhams.OrderBy(sp => sp.TenSP).ToList();

            var model = new HoaDon();
            if (!LaAdmin() && nvDangNhap != null) model.MaNV = nvDangNhap.MaNV;
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            HoaDon hoaDon,
            int[] sanPhamIds,
            int[] soLuongs
        )
        {
            if (!ChiChoNhanVien())
                return KhongCoQuyen();

            if (hoaDon.ChiTietHoaDons == null)
                hoaDon.ChiTietHoaDons = new List<ChiTietHoaDon>();

            decimal tongTienHang = 0m;

            if (sanPhamIds != null && soLuongs != null)
            {
                for (int i = 0; i < sanPhamIds.Length; i++)
                {
                    int idSp = sanPhamIds[i];
                    int sl = (i < soLuongs.Length) ? soLuongs[i] : 0;

                    if (idSp <= 0 || sl <= 0)
                        continue;

                    var sp = db.SanPhams.Find(idSp);
                    if (sp == null)
                        continue;

                    decimal donGia = sp.Gia;

                    var ct = new ChiTietHoaDon
                    {
                        MaSP = sp.MaSP,
                        SoLuong = sl,
                        DonGia = donGia
                    };

                    hoaDon.ChiTietHoaDons.Add(ct);
                    tongTienHang += donGia * sl;
                }
            }

            if (!hoaDon.ChiTietHoaDons.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 món ăn với số lượng > 0.");
            }

            if (string.IsNullOrWhiteSpace(hoaDon.DiaChi))
            {
                hoaDon.DiaChi = "Tại quầy";
            }

            if (!hoaDon.MaTT.HasValue || hoaDon.MaTT.Value == 0)
            {
                var daGiao = db.TinhTrangs.FirstOrDefault(t => t.TenTT == "Đã giao");
                if (daGiao != null)
                    hoaDon.MaTT = daGiao.MaTT;
            }


            if (ModelState.IsValid)
            {
                if (!hoaDon.NgayLap.HasValue || hoaDon.NgayLap.Value == default(DateTime))
                {
                    hoaDon.NgayLap = DateTime.Now;
                }

                decimal giam = 0m;
                if (hoaDon.MaKM.HasValue)
                {
                    var km = db.KhuyenMais.Find(hoaDon.MaKM.Value);
                    if (km != null && km.PhanTramGiam.HasValue)
                    {
                        decimal pt = (decimal)km.PhanTramGiam.Value;
                        giam = Math.Round(tongTienHang * pt / 100m, 0);
                    }
                }

                const decimal SHIP_FEE = 10000m;
                decimal phiShipTien = hoaDon.PhiShip == true ? SHIP_FEE : 0m;
                hoaDon.TongTien = tongTienHang - giam + phiShipTien;
                db.HoaDons.Add(hoaDon);
                db.SaveChanges();
                TempData["Success"] = "Tạo hóa đơn thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.MaKH = new SelectList(db.KhachHangs, "MaKH", "TenKH", hoaDon.MaKH);
            ViewBag.MaNV = new SelectList(db.NhanViens, "MaNV", "TenNV", hoaDon.MaNV);
            ViewBag.MaKM = new SelectList(db.KhuyenMais, "MaKM", "TenKM", hoaDon.MaKM);
            ViewBag.MaTT = new SelectList(db.TinhTrangs, "MaTT", "TenTT", hoaDon.MaTT);
            ViewBag.SanPhams = db.SanPhams.OrderBy(sp => sp.TenSP).ToList();

            return View(hoaDon);
        }


        // ====== SỬA HÓA ĐƠN ======

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
                return HttpNotFound();

            if (!LaAdmin())
            {
                var nv = LayNhanVienDangNhap();
                if (nv == null || !hoaDon.MaNV.HasValue || hoaDon.MaNV.Value != nv.MaNV)
                    return KhongCoQuyen();
            }


            ViewBag.LaAdmin = LaAdmin();

            ViewBag.MaKH = new SelectList(db.KhachHangs, "MaKH", "TenKH", hoaDon.MaKH);
            ViewBag.MaNV = new SelectList(db.NhanViens, "MaNV", "TenNV", hoaDon.MaNV);
            ViewBag.MaKM = new SelectList(db.KhuyenMais, "MaKM", "TenKM", hoaDon.MaKM);
            ViewBag.MaTT = new SelectList(db.TinhTrangs, "MaTT", "TenTT", hoaDon.MaTT);

            return View(hoaDon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
        [Bind(Include = "MaHD,MaKH,MaNV,MaKM,MaTT,NgayLap,PhiShip,DiaChi")]
        HoaDon hoaDon)
        {
            if (string.IsNullOrWhiteSpace(hoaDon.DiaChi))
            {
                ModelState.AddModelError("DiaChi", "Vui lòng nhập địa chỉ giao hàng.");
            }

            if (ModelState.IsValid)
            {
                var existing = db.HoaDons
                                 .Include(h => h.ChiTietHoaDons)
                                 .FirstOrDefault(h => h.MaHD == hoaDon.MaHD);

                if (existing == null)
                    return HttpNotFound();

                if (!LaAdmin())
                {
                    var nv = LayNhanVienDangNhap();
                    if (nv == null || !existing.MaNV.HasValue || existing.MaNV.Value != nv.MaNV)
                    {
                        TempData["Error"] = "Bạn không có quyền sửa hóa đơn này.";
                        return RedirectToAction("Index");
                    }
                }

                if (LaAdmin())
                {
                    existing.MaKH = hoaDon.MaKH;
                    existing.MaNV = hoaDon.MaNV;
                    existing.MaKM = hoaDon.MaKM;
                    existing.NgayLap = hoaDon.NgayLap ?? existing.NgayLap ?? DateTime.Now;
                    existing.DiaChi = hoaDon.DiaChi;
                }
                existing.MaTT = hoaDon.MaTT;
                existing.PhiShip = hoaDon.PhiShip;
                decimal tongTienHang = 0m;
                if (existing.ChiTietHoaDons != null)
                {
                    foreach (var ct in existing.ChiTietHoaDons)
                    {
                        tongTienHang += ct.DonGia * ct.SoLuong;
                    }
                }

                decimal giam = 0m;
                if (existing.MaKM.HasValue)
                {
                    var km = db.KhuyenMais.Find(existing.MaKM.Value);
                    if (km != null && km.PhanTramGiam.HasValue)
                    {
                        decimal pt = (decimal)km.PhanTramGiam.Value;
                        giam = Math.Round(tongTienHang * pt / 100m, 0);
                    }
                }

                const decimal SHIP_FEE = 10000m;
                bool coShip = existing.PhiShip ?? false;
                decimal phiShipTien = coShip ? SHIP_FEE : 0m;
                existing.TongTien = tongTienHang - giam + phiShipTien;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật hóa đơn thành công.";
                return RedirectToAction("Details", new { id = existing.MaHD });
            }
            ViewBag.LaAdmin = LaAdmin();
            //ViewBag.MaKH = new SelectList(db.KhachHangs, "MaKH", "TenKH", hoaDon.MaKH);
            //ViewBag.MaNV = new SelectList(db.NhanViens, "MaNV", "TenNV", hoaDon.MaNV);
            //ViewBag.MaKM = new SelectList(db.KhuyenMais, "MaKM", "TenKM", hoaDon.MaKM);
            //ViewBag.MaTT = new SelectList(db.TinhTrangs, "MaTT", "TenTT", hoaDon.MaTT);

            //return View(hoaDon);

            HoaDon hd = db.HoaDons
                         .Include(h => h.KhachHang)
                         .Include(h => h.NhanVien)
                         .Include(h => h.KhuyenMai)
                         .Include(h => h.TinhTrang)
                         .FirstOrDefault(h => h.MaHD == hoaDon.MaHD);

            if (hd == null)
                return HttpNotFound();

            hd.DiaChi = hoaDon.DiaChi;

            ViewBag.MaKH = new SelectList(db.KhachHangs, "MaKH", "TenKH", hd.MaKH);
            ViewBag.MaNV = new SelectList(db.NhanViens, "MaNV", "TenNV", hd.MaNV);
            ViewBag.MaKM = new SelectList(db.KhuyenMais, "MaKM", "TenKM", hd.MaKM);
            ViewBag.MaTT = new SelectList(db.TinhTrangs, "MaTT", "TenTT", hd.MaTT);

            return View(hd);
        }

        // ====== XÓA HÓA ĐƠN (CHỈ ADMIN) ======

        public ActionResult Delete(int? id)
        {
            if (!LaAdmin()) return KhongCoQuyen();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            HoaDon hoaDon = db.HoaDons
                              .Include(h => h.KhachHang)
                              .FirstOrDefault(h => h.MaHD == id);
            if (hoaDon == null)
                return HttpNotFound();

            return View(hoaDon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!LaAdmin()) return KhongCoQuyen();
            var hoaDon = db.HoaDons
                           .Include(h => h.ChiTietHoaDons)
                           .FirstOrDefault(h => h.MaHD == id);

            if (hoaDon == null)
                return HttpNotFound();

            if (hoaDon.ChiTietHoaDons != null && hoaDon.ChiTietHoaDons.Any())
            {
                db.ChiTietHoaDons.RemoveRange(hoaDon.ChiTietHoaDons);
            }
            db.HoaDons.Remove(hoaDon);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa hóa đơn.";
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
