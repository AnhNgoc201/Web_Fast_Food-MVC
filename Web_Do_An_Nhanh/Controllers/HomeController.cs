using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;

namespace Web_Do_An_Nhanh.Controllers
{
    public class HomeController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult _DanhSachNgauNhien()
        {
            var ngauNhien = db.SanPhams
                              .OrderBy(x => Guid.NewGuid())
                              .Take(8)
                              .ToList();

            return PartialView("_DanhSachNgauNhien", ngauNhien);
        }

    }
}