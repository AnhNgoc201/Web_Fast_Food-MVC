using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Do_An_Nhanh.Models
{
    public class CartItem
    {
        public int iMaSP { get; set; }
        public string sTenSP { get; set; }
        public string sHinhAnh { get; set; }
        public decimal dDonGia { get; set; }
        public int iSoLuong { get; set; }
        public decimal ThanhTien
        {
            get { return iSoLuong * dDonGia; }
        }
        JOLLIBEEEntities db = new JOLLIBEEEntities();
        public CartItem(int MaSP)
        {
            SanPham sp = db.SanPhams.Single(n => n.MaSP == MaSP);
            if (sp != null)
            {
                iMaSP = sp.MaSP;
                sTenSP = sp.TenSP;
                sHinhAnh = sp.HinhAnh;
                dDonGia = decimal.Parse(sp.Gia.ToString());
                iSoLuong = 1;
            }
        }
    }
}