CREATE DATABASE JOLLIBEE;
GO
USE JOLLIBEE;
GO

CREATE TABLE DanhMuc (
    MaDM INT IDENTITY(1,1) PRIMARY KEY,
    TenDM NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255)
);

CREATE TABLE SanPham (
    MaSP INT IDENTITY(1,1) PRIMARY KEY,
    MaDM INT NOT NULL,
    TenSP NVARCHAR(100) NOT NULL,
    Gia DECIMAL(10,2) NOT NULL,
    HinhAnh NVARCHAR(255),
    TrangThai BIT DEFAULT 1,       
    MoTa NVARCHAR(255),            
    FOREIGN KEY (MaDM) REFERENCES DanhMuc(MaDM)
);


CREATE TABLE VaiTro (
    MaVT INT IDENTITY(1,1) PRIMARY KEY,
    TenVT NVARCHAR(50) NOT NULL
);

CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    TenNV NVARCHAR(100) NOT NULL,
    MaVT INT NOT NULL,
    Email NVARCHAR(100),
    SDT NVARCHAR(20),
    MatKhau NVARCHAR(100),
    FOREIGN KEY (MaVT) REFERENCES VaiTro(MaVT)
);

CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    SDT NVARCHAR(20),
    DiaChi NVARCHAR(255),
    MatKhau NVARCHAR(100)
);
ALTER TABLE KhachHang 
ADD ResetOtp NVARCHAR(10),
    ResetExpired DATETIME;

CREATE TABLE TinhTrang (
    MaTT INT IDENTITY(1,1) PRIMARY KEY,
    TenTT NVARCHAR(50) NOT NULL
);

CREATE TABLE KhuyenMai (
    MaKM INT IDENTITY(1,1) PRIMARY KEY,
    TenKM NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255),
    PhanTramGiam FLOAT,
    NgayBatDau DATETIME,
    NgayKetThuc DATETIME
);

CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NULL,
    MaNV INT NULL,
	MaKM INT NULL,
    MaTT INT DEFAULT 1,
    NgayLap DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(10,2) DEFAULT 0,
	PhiShip BIT DEFAULT 0,
    DiaChi NVARCHAR(255),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV),
    FOREIGN KEY (MaKM) REFERENCES KhuyenMai(MaKM),
    FOREIGN KEY (MaTT) REFERENCES TinhTrang(MaTT)
);

CREATE TABLE ChiTietHoaDon (
    MaHD INT NOT NULL,
    MaSP INT NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1,
    DonGia DECIMAL(10,2) NOT NULL,
	PRIMARY KEY (MaHD, MaSP),
    FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
----Dữ liệu----
INSERT INTO DanhMuc (TenDM, MoTa) VALUES
(N'Gà giòn vui vẻ', N'Món gà rán Jollibee'),
(N'Gà sốt cay', N'Món gà rán sốt cay Jollibee'),
(N'Mì ý Jolly', N'Món Mì ý Jollibee'),
(N'Burger/Cơm', N'Món burger/cơm Jollibee'),
(N'Phần ăn phụ', N'Phần ăn phụ Jollibee'),
(N'Món tráng miệng', N'Món tráng miệng Jollibee'),
(N'Nước uống', N'Nước uống Jollibee');
INSERT INTO SanPham (MaDM, TenSP, Gia, HinhAnh, TrangThai, MoTa) VALUES
(1, N'2 miếng Gà Giòn Vui Vẻ', 66000, N'ga_gion_2_mieng.jpg', 1, N'2 miếng Gà Giòn Vui Vẻ + tương chua ngọt'),
(1, N'4 miếng Gà Giòn Vui Vẻ', 126000, N'ga_gion_4_mieng.jpg', 1, N'4 miếng Gà Giòn Vui Vẻ + tương chua ngọt'),
(1, N'6 miếng Gà Giòn Vui Vẻ', 188000, N'ga_gion_6_mieng.jpg', 1, N'6 miếng Gà Giòn Vui Vẻ + tương chua ngọt'),
(1, N'2 Gà Giòn Vui Vẻ + 1 Khoai tây chiên vừa + 1 Nước ngọt', 91000, N'combo_2ga1khoai1nuoc.jpg', 1, N'Combo 2 gà + khoai vừa + nước ngọt'),
(1, N'1 Gà Giòn Vui Vẻ + 1 Khoai tây chiên vừa + 1 Nước ngọt', 58000, N'combo_1ga1khoai1nuoc.jpg', 1, N'1 gà + khoai + nước'),
(1, N'1 Cơm Gà Giòn Vui Vẻ + 1 Súp bí đỏ + 1 Nước ngọt', 63000, N'com_ga_gion_sup_bido.jpg', 1, N'Cơm gà giòn + súp bí đỏ + nước ngọt'),
(1, N'1 Cơm Gà Giòn Vui Vẻ + 1 Nước ngọt + 1 Tương Chua Ngọt', 58000, N'com_ga_gion_nuoc_tuong.jpg', 1, N'Cơm gà giòn + nước + tương chua ngọt'),
(1, N'1 Cơm Gà Giòn Vui Vẻ', 48000, N'com_ga_gion.jpg', 1, N'Cơm gà giòn vui vẻ'),
(1, N'1 miếng Gà Giòn Vui Vẻ', 33000, N'ga_gion_1_mieng.jpg', 1, N'1 miếng Gà Giòn Vui Vẻ'),

(2, N'2 miếng Gà Sốt Cay', 70000, N'ga_sot_cay_2mieng.jpg', 1, N'2 miếng gà sốt cay theo menu Jollibee Việt Nam'),  
(2, N'2 Gà Sốt Cay + 1 Khoai vừa + 1 Nước ngọt', 95000, N'combo_2ga_sotcay_khoai_nuoc.jpg', 1, N'2 gà sốt cay + khoai vừa + nước ngọt + tương Cà'),  
(2, N'1 Gà Sốt Cay + 1 Khoai vừa + 1 Nước ngọt', 60000, N'combo_1ga_sotcay_khoai_nuoc.jpg', 1, N'1 gà sốt cay + khoai vừa + nước ngọt + tương Cà'),  
(2, N'1 Cơm Gà Sốt Cay + 1 Súp bí đỏ + 1 Nước ngọt', 65000, N'com_ga_sotcay_sup_bido_nuoc.jpg', 1, N'Cơm gà sốt cay + súp bí đỏ + nước ngọt'),  
(2, N'1 Cơm Gà Sốt Cay + 1 Nước ngọt', 60000, N'com_ga_sotcay_nuoc.jpg', 1, N'Cơm gà sốt cay + nước ngọt'),  
(2, N'1 Cơm Gà Sốt Cay', 50000, N'com_ga_sotcay.jpg', 1, N'Cơm gà sốt cay'),  
(2, N'1 miếng Gà Sốt Cay', 35000, N'ga_sot_cay_1mieng.jpg', 1, N'1 miếng gà sốt cay'),

(3, N'Mì Ý Jolly vừa (bò bằm)', 35000, N'miy_jolly.jpg', 1, N'Mì Ý sốt bò bằm kích cỡ vừa'),
(3, N'Mì Ý Jolly lớn (bò bằm)', 45000, N'miy_jolly.jpg', 1, N'Mì Ý sốt bò bằm kích cỡ lớn'),
(3, N'1 Mì Ý sốt bò bằm + 1 miếng Gà Giòn Vui Vẻ + 1 Nước ngọt', 65000, N'combo_my_bo_gagion_nuoc.jpg', 1, N'Combo mì bò bằm + gà + nước ngọt'),
(3, N'1 Mì Ý sốt bò bằm + 1 Khoai tây vừa + 1 Nước ngọt', 60000, N'combo_my_bo_khoai_nuoc.jpg', 1, N'Combo mì bò bằm + khoai vừa + nước'),
(3, N'1 Mì Ý sốt bò bằm + 1 Nước ngọt', 50000, N'my_bo_nuoc.jpg', 1, N'Mì Ý sốt bò bằm + nước ngọt'),

(4, N'Cơm Gà Mắm Tỏi', 35000, N'com_ga_mam_toi.jpg', 1, N'Cơm gà mắm tỏi thơm ngon, giòn rụm'),
(4, N'1 Cơm gà mắm tỏi + 1 Nước ngọt', 45000, N'combo_com_ga_mam_toi_nuoc.jpg', 1, N'Combo cơm gà mắm tỏi kèm nước ngọt'),
(4, N'Burger Tôm', 40000, N'burger_tom.jpg', 1, N'Burger tôm giòn đặc trưng Jollibee'),
(4, N'1 Burger Tôm + 1 Nước ngọt', 50000, N'combo_burger_tom_nuoc.jpg', 1, N'Combo burger tôm + nước ngọt'),
(4, N'1 Burger Tôm + 1 Khoai tây chiên vừa + 1 Nước ngọt', 65000, N'combo_burger_tom_khoai_nuoc.jpg', 1, N'Burger tôm + khoai vừa + nước'),
(4, N'Jolly Hotdog', 25000, N'jolly_hotdog.jpg', 1, N'Hotdog Jolly nổi tiếng'),
(4, N'1 Jolly Hotdog + 1 Nước ngọt', 35000, N'combo_jolly_hotdog_nuoc.jpg', 1, N'Combo Jolly Hotdog + nước ngọt'),
(4, N'1 Jolly Hotdog + 1 Khoai tây chiên vừa + 1 Nước ngọt', 50000, N'combo_jolly_hotdog_khoai_nuoc.jpg', 1, N'Hotdog + khoai + nước'),
(4, N'Sandwich Gà Giòn', 30000, N'sandwich_ga_gion.jpg', 1, N'Sandwich gà giòn thơm ngon'),
(4, N'1 Sandwich Gà Giòn + 1 Nước ngọt', 40000, N'combo_sandwich_ga_gion_nuoc.jpg', 1, N'Combo sandwich gà + nước ngọt'),
(4, N'1 Sandwich Gà Giòn + 1 Khoai tây chiên vừa + 1 Nước ngọt', 55000, N'combo_sandwich_ga_gion_khoai_nuoc.jpg', 1, N'Sandwich gà + khoai + nước'),

(5, N'Tương chua ngọt (2 gói)', 1000, N'tuong_chua_ngot_2_goi.jpg', 1, N'Thêm 2 gói tương chua ngọt'),  
(5, N'Tương cà (2 gói)', 1000, N'tuong_ca_2_goi.jpg', 1, N'Thêm 2 gói tương cà'),  
(5, N'Khoai tây lắc vị BBQ lớn', 35000, N'khoai_lac_bbq.jpg', 1, N'Khoai tây lắc vị BBQ lớn, đậm vị'),  
(5, N'Khoai tây lắc vị BBQ vừa', 25000, N'khoai_lac_bbq.jpg', 1, N'Khoai tây lắc vị BBQ vừa'),  
(5, N'Khoai tây chiên lớn + 2 tương cà', 25000, N'khoai_chien.jpg', 1, N'Khoai tây chiên lớn kèm 2 gói tương cà'),  
(5, N'Khoai tây chiên vừa + 1 tương cà', 20000, N'khoai_chien.jpg', 1, N'Khoai tây chiên vừa kèm 1 gói tương cà'),  
(5, N'Súp bí đỏ', 15000, N'sup_bi_do.jpg', 1, N'Súp bí đỏ thơm ngon'),  
(5, N'Cơm trắng', 10000, N'com_trang.jpg', 1, N'Cơm trắng dùng kèm món chính'),

(6, N'Bánh xoài đào', 15000, N'banh_xoai_dao.jpg', 1, N'Bánh xoài đào thơm mát'),
(6, N'Tropical Sundae', 20000, N'tropical_sundae.jpg', 1, N'Kem Tropical Sundae tươi mát'),
(6, N'Kem Sundae Dâu', 15000, N'kem_sundae_dau.jpg', 1, N'Kem Sundae vị dâu'),
(6, N'Kem Sundae Socola', 15000, N'kem_sundae_socola.jpg', 1, N'Kem Sundae vị socola'),
(6, N'Kem Socola (Cúp)', 7000, N'kem_socola_cup.jpg', 1, N'Kem socola cỡ nhỏ trong cốc'),
(6, N'Kem Sữa Tươi (Cúp)', 5000, N'kem_sua_tuoi_cup.jpg', 1, N'Kem sữa tươi cỡ nhỏ trong cốc'),

(7, N'Trá Chanh Hạt Chia', 20000, N'tra_chanh_hat_chia.jpg', 1, N'Trá chanh + hạt chia mát lạnh'),  
(7, N'Nước ép Xoài Đào', 20000, N'nuoc_ep_xoai_dao.jpg', 1, N'Nước ép xoài + đào tươi'),  
(7, N'Pepsi lớn', 17000, N'pepsi.jpg', 1, N'Pepsi kích cỡ lớn'),  
(7, N'Pepsi vừa', 12000, N'pepsi.jpg', 1, N'Pepsi kích cỡ vừa'),  
(7, N'Mirinda lớn', 17000, N'mirinda.jpg', 1, N'Mirinda lớn, vị cam hoặc vị khác'),  
(7, N'Mirinda vừa', 12000, N'mirinda.jpg', 1, N'Mirinda vừa'),  
(7, N'7UP lớn', 17000, N'7up.jpg', 1, N'7UP lớn'),  
(7, N'7UP vừa', 12000, N'7up.jpg', 1, N'7UP vừa'),  
(7, N'Cacao sữa đá lớn', 25000, N'cacao_sua_da.jpg', 1, N'Cacao sữa đá kích cỡ lớn'),  
(7, N'Cacao sữa đá vừa', 20000, N'cacao_sua_da.jpg', 1, N'Cacao sữa đá cỡ vừa'),  
(7, N'Nước suối', 8000, N'nuoc_suoi.jpg', 1, N'Nước suối đóng chai');
INSERT INTO VaiTro (TenVT) VALUES
(N'Admin'), (N'Order'), (N'CSKH');
INSERT INTO NhanVien (TenNV, MaVT, Email, SDT, MatKhau) VALUES
(N'Nguyen Van A', 1, N'admin@jollibee.vn', N'0909000001', N'123456'),
(N'Tran Thi B', 2, N'nv1@jollibee.vn', N'0909000002', N'123456');
INSERT INTO KhachHang (TenKH, Email, SDT, DiaChi, MatKhau) VALUES
(N'Le Van C', N'levc@gmail.com', N'0911000001', N'123 Đường A, TP HCM', N'123456'),
(N'Pham Thi D', N'ptd@gmail.com', N'0911000002', N'456 Đường B, TP HCM', N'123456');
INSERT INTO TinhTrang (TenTT) VALUES
(N'Đặt thành công'),
(N'Chờ xác nhận'),
(N'Đang làm món'),
(N'Hoàn thành chế biến'),
(N'Đã giao'),
(N'Đang vận chuyển'),
(N'Đã hủy');
INSERT INTO KhuyenMai (TenKM, MoTa, PhanTramGiam, NgayBatDau, NgayKetThuc) VALUES
(N'Giảm 10% cho hóa đơn', N'Áp dụng từ thứ 2 đến thứ 6 tuần thứ 3 tháng 12', 10, '2025-14-15', '2025-12-19'),
(N'Giảm 15% giữa tháng 12', N'Khuyến mãi áp dụng từ 14 đến 20/12', 15, '2025-12-14', '2025-12-20'),
(N'Giảm 20% cuối tuần trước lễ Giáng Sinh', N'Từ thứ 6 đến chủ nhật trước Giáng Sinh', 20, '2025-12-14', '2025-12-21'),
(N'Giảm 25% tuần Giáng Sinh', N'Từ 14 đến 26/12', 25, '2025-12-14', '2025-12-26'),
(N'Giảm 30% tuần cuối cùng của tháng', N'Từ 27 đến 31/12', 30, '2025-12-27', '2025-12-31');

INSERT INTO HoaDon (MaKH, MaNV, MaKM, MaTT, NgayLap, TongTien, PhiShip, DiaChi) VALUES
(1, 1, NULL, 1, GETDATE(), 66000, 0, N'123 Lê Lợi, Q.1, TP.HCM'),
(2, 2, 1,   1, GETDATE(), 126000, 1, N'55 Nguyễn Trãi, Q.5, TP.HCM'),
(1, 1, NULL, 2, GETDATE(), 83000, 0, N'10 Đặng Văn Bi, TP.Thủ Đức'),
(2, 2, NULL, 3, GETDATE(), 50000, 1, N'22 Hai Bà Trưng, Q.3'),
(1, 1, NULL, 4, GETDATE(), 40000, 0, N'99 Trần Hưng Đạo, Q.1');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
(1, 1, 1, 66000);   
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
(2, 2, 1, 126000);   
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
(3, 3, 1, 83000);   
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
(4, 10, 1, 50000);   
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
(5, 11, 1, 40000);  