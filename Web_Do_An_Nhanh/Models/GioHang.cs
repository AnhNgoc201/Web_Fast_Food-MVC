using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Do_An_Nhanh.Models
{
    public class GioHang
    {
        public List<CartItem> lst;
        public GioHang()
        {
            lst = new List<CartItem>();
        }
        public GioHang(List<CartItem> lstGH)
        {
            lst = lstGH;
        }
        public float PhanTramGiam { get; set; }
        public string TenKM { get; set; }
        public int? MaKM { get; set; }
        public bool PhiShip { get; set; } = true;
        public int SoMatHang()
        {
            return lst.Count;
        }
        public int TongSLHang()
        {
            return lst.Sum(n => n.iSoLuong);
        }
        public decimal TongThanhTien()
        {
            return lst.Sum(n => n.ThanhTien);
        }
        public int Them(int iMaSP, int soluong = 1)
        {
            CartItem sanpham = lst.Find(n => n.iMaSP == iMaSP);

            if (sanpham == null)
            {
                CartItem sp = new CartItem(iMaSP);
                if (sp == null)
                    return -1;

                sp.iSoLuong = soluong;
                lst.Add(sp);
            }
            else
            {
                sanpham.iSoLuong += soluong;
            }

            return 1;
        }
        public void Xoa(int msp)
        {
            var sp = lst.FirstOrDefault(x => x.iMaSP == msp);
            if (sp != null)
                lst.Remove(sp);
        }

        public void CapNhat(int msp, int soLuong)
        {
            var sp = lst.FirstOrDefault(x => x.iMaSP == msp);
            if (sp != null)
                sp.iSoLuong = soLuong;
        }

    }
}