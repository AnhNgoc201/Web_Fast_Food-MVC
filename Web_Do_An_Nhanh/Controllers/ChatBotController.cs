using System;
using System.Linq;
using System.Web.Mvc;
using Web_Do_An_Nhanh.Models;
using System.Collections.Generic;

namespace Web_Do_An_Nhanh.Controllers
{
    public class ChatBotController : Controller
    {
        private JOLLIBEEEntities db = new JOLLIBEEEntities();

        // GET: ChatBot
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Ask(string question)
        {
            string keyword = question.ToLower();
            var results = new List<object>();

            // 1️⃣ Chào hỏi / cảm ơn
            if (keyword.Contains("xin chào") || keyword.Contains("hello") || keyword.Contains("hi"))
            {
                results.Add(new { TenSP = "Xin chào! Mình là chatbot Jollibee. Bạn muốn xem món gì?", Gia = 0, MoTa = "", HinhAnh = "" });
            }
            else if (keyword.Contains("cảm ơn") || keyword.Contains("thank"))
            {
                results.Add(new { TenSP = "Bạn không có gì 😊. Mình luôn sẵn sàng hỗ trợ!", Gia = 0, MoTa = "", HinhAnh = "" });
            }
            // 2️⃣ Khuyến mãi
            else if (keyword.Contains("khuyến mãi") || keyword.Contains("ưu đãi") || keyword.Contains("promotion"))
            {
                var today = DateTime.Now;

                var km = db.KhuyenMais
                           .Where(k => k.NgayBatDau <= today && k.NgayKetThuc >= today)
                           .Select(k => new { k.TenKM, k.MoTa, k.PhanTramGiam })
                           .Take(3)
                           .ToList();

                if (!km.Any())
                {
                    results.Add(new { TenSP = "Hiện tại không có khuyến mãi nào.", Gia = 0, MoTa = "", HinhAnh = "" });
                }
                else
                {
                    foreach (var k in km)
                    {
                        results.Add(new
                        {
                            TenSP = k.TenKM,
                            Gia = 0,
                            MoTa = $"{k.MoTa} - Giảm {k.PhanTramGiam}%",
                            HinhAnh = ""
                        });
                    }
                }
            }
            else
            {
                var productsQuery = db.SanPhams.AsQueryable();

                if (keyword.Contains("gà giòn") || keyword.Contains("gà sốt cay"))
                {
                    productsQuery = productsQuery.Where(p => p.TenSP.ToLower().Contains("gà giòn") || p.TenSP.ToLower().Contains("gà sốt cay"));
                }
                else if (keyword.Contains("combo"))
                {
                    productsQuery = productsQuery.Where(p => p.TenSP.ToLower().Contains("combo"));
                }
                else if (keyword.Contains("nước") || keyword.Contains("pepsi") || keyword.Contains("mirinda") || keyword.Contains("7up") || keyword.Contains("cacao"))
                {
                    productsQuery = productsQuery.Where(p => p.MaDM == 7); 
                }
                else if (keyword.Contains("khoai") || keyword.Contains("súp") || keyword.Contains("cơm"))
                {
                    productsQuery = productsQuery.Where(p => p.MaDM == 4 || p.MaDM == 5); 
                }
                else if (keyword.Contains("kem") || keyword.Contains("bánh"))
                {
                    productsQuery = productsQuery.Where(p => p.MaDM == 6); 
                }
                else
                {
                    results.Add(new { TenSP = "Xin lỗi, mình chưa hiểu. Bạn có thể nói rõ hơn?", Gia = 0, MoTa = "", HinhAnh = "" });
                    return Json(results, JsonRequestBehavior.AllowGet);
                }

                var products = productsQuery
                               .Select(p => new { p.TenSP, p.Gia, p.MoTa, p.HinhAnh })
                               .Take(3)
                               .ToList<object>();

                if (!products.Any())
                {
                    results.Add(new { TenSP = "Xin lỗi, mình chưa tìm thấy sản phẩm phù hợp.", Gia = 0, MoTa = "", HinhAnh = "" });
                }
                else
                {
                    results.AddRange(products);
                }
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}
