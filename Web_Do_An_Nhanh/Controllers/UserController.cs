using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class UserController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            string email = form["Email"];
            string password = form["Password"];

            var user = db.KhachHangs.FirstOrDefault(x => x.Email == email);

            if (user != null && user.MatKhau == password)
            {
                Session["User"] = user;
                TempData["Message"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "SanPhams");
            }

            ViewBag.Error = "Sai email hoặc mật khẩu!";
            return View();
        }

        public ActionResult Logout()
        {
            Session["User"] = null;
            Session["SoLuongGioHang"] = null;
            TempData["Message"] = "Đã đăng xuất!";
            return RedirectToAction("Index", "SanPhams");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(FormCollection form)
        {
            string tenKH = form["TenKH"];
            string email = form["Email"];
            string matKhau = form["MatKhau"];
            string sdt = form["SDT"];
            string diaChi = form["DiaChi"];

            if (string.IsNullOrEmpty(tenKH) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(matKhau) ||
                string.IsNullOrEmpty(sdt) ||
                string.IsNullOrEmpty(diaChi))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (db.KhachHangs.Any(x => x.Email == email))
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View();
            }

            KhachHang kh = new KhachHang
            {
                TenKH = tenKH,
                Email = email,
                MatKhau = matKhau, 
                SDT = sdt,
                DiaChi = diaChi
            };

            db.KhachHangs.Add(kh);
            db.SaveChanges();

            TempData["Message"] = "Đăng ký thành công! Hãy đăng nhập.";
            return RedirectToAction("Login");
        }

        public ActionResult LichSuMuaHang()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login");

            var kh = (KhachHang)Session["User"];

            var donHang = db.HoaDons
                            .Where(x => x.MaKH == kh.MaKH)
                            .OrderByDescending(x => x.NgayLap)
                            .ToList();

            return View(donHang);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendOtp(string email)
        {
            var user = db.KhachHangs.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại!";
                return View("ForgotPassword");
            }

            string otp = new Random().Next(100000, 999999).ToString();

            user.ResetOtp = otp;
            user.ResetExpired = DateTime.Now.AddMinutes(5);
            db.SaveChanges();

            SendEmail(email, "Mã OTP đặt lại mật khẩu",
                "Mã OTP của bạn là: " + otp + "\nHiệu lực trong 5 phút.");

            TempData["Email"] = email;
            return RedirectToAction("VerifyOtp");
        }

        public ActionResult VerifyOtp()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [HttpPost]
        public ActionResult VerifyOtp(string email, string otp)
        {
            var user = db.KhachHangs.FirstOrDefault(x =>
                x.Email == email &&
                x.ResetOtp == otp &&
                x.ResetExpired > DateTime.Now);

            if (user == null)
            {
                ViewBag.Error = "OTP sai hoặc đã hết hạn!";
                ViewBag.Email = email;
                return View();
            }

            TempData["Email"] = email;
            return RedirectToAction("ResetPassword");
        }

        public ActionResult ResetPassword()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string email, string password)
        {
            var user = db.KhachHangs.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Lỗi tài khoản!";
                return View();
            }

            user.MatKhau = password; 
            user.ResetOtp = null;
            user.ResetExpired = null;
            db.SaveChanges();

            TempData["Message"] = "Đặt lại mật khẩu thành công!";
            return RedirectToAction("Login");
        }

        public void SendEmail(string to, string subject, string body)
        {
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    
                )
            };

            MailMessage mail = new MailMessage(
                "phamanhngoc201pan@gmail.com",
                to,
                subject,
                body
            );

            smtp.Send(mail);
        }
    }
}
