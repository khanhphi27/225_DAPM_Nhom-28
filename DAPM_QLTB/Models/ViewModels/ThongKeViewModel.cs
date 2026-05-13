using System;
using System.Collections.Generic;

namespace QLTB.Models
{
    public class BGHDashboardViewModel
    {
        public int TongThietBi    { get; set; }
        public int HoatDong       { get; set; }
        public int BaoTri         { get; set; }
        public int Hong           { get; set; }
        public int ChoDeXuatDuyet { get; set; }
        public List<HoatDongGanDayViewModel> HoatDongGanDay { get; set; }
    }

    public class ThongKeTaiSanViewModel
    {
        public int     TongThietBi    { get; set; }
        public int     HoatDong       { get; set; }
        public int     BaoTri         { get; set; }
        public int     Hong           { get; set; }
        public decimal TongGiaTri     { get; set; }
        public decimal ChiPhiBaoTri   { get; set; }
        public int     TongBaoHong    { get; set; }
        public List<ThongKeTheoKhoaViewModel>    TheoKhoa     { get; set; }
        public List<ThongKeTheoDanhMucViewModel> TheoDanhMuc  { get; set; }
        public List<KiemKeListViewModel>         LichSuKiemKe { get; set; }
        public List<ThietBiCanChuYViewModel>     CanChuY      { get; set; }
    }

    public class ThietBiCanChuYViewModel
    {
        public string   ID_ThietBi   { get; set; }
        public string   TenTB        { get; set; }
        public string   TrangThaiTB  { get; set; }
        public string   TenDanhMuc   { get; set; }
        public string   KhoaPhongBan { get; set; }
        public decimal? Gia          { get; set; }
        public string   MoTaHong     { get; set; }
        public DateTime? NgayBaoHong { get; set; }
    }

    public class ThongKeTheoKhoaViewModel
    {
        public string  TenKhoa  { get; set; }
        public int     Tong     { get; set; }
        public int     HoatDong { get; set; }
        public int     BaoTri   { get; set; }
        public int     Hong     { get; set; }
        public decimal TongGia  { get; set; }
    }

    public class ThongKeTheoDanhMucViewModel
    {
        public string TenDanhMuc { get; set; }
        public int    SoLuong    { get; set; }
    }

    public class KiemKeListViewModel
    {
        public string   ID_KiemKe     { get; set; }
        public DateTime NgayKiemKe    { get; set; }
        public string   NguoiThucHien { get; set; }
        public string   TrangThai     { get; set; }
        public string   GhiChu        { get; set; }
        public int      TongThietBiKK { get; set; }
    }

    public class TheDoiThietBiViewModel
    {
        public string    ID_ThietBi   { get; set; }
        public string    TenTB        { get; set; }
        public string    TrangThaiTB  { get; set; }
        public string    TenDanhMuc   { get; set; }
        public string    KhoaPhongBan { get; set; }
        public decimal?  Gia          { get; set; }
        public string    NhaCungCap   { get; set; }
        public string    Phong        { get; set; }
        public string    MoTaHong     { get; set; }
        public DateTime? NgayBaoHong  { get; set; }
        public string    NguoiBaoHong { get; set; }
    }

    public class KHTCDashboardViewModel
    {
        public decimal TongGiaTri  { get; set; }
        public decimal TongSuaChua { get; set; }
        public int     ChoDuyet    { get; set; }
        public List<DeXuatViewModel> HoatDongGanDay { get; set; }
        public KHTCDashboardViewModel() { HoatDongGanDay = new List<DeXuatViewModel>(); }
    }

    public class ChiPhiViewModel
    {
        public string   ID_GhiNhan    { get; set; }
        public DateTime NgayThucHien  { get; set; }
        public decimal  ChiPhiThucTe  { get; set; }
        public string   DonViThucHien { get; set; }
        public string   KetQua        { get; set; }
    }

    public class BaoCaoTaiSanViewModel
    {
        public string ID_ThietBi { get; set; }
        public string TenTB { get; set; }
        public string SoSeri { get; set; }
        public string ThongSoKT { get; set; }
        public string TenDanhMuc { get; set; }
        public string TenPhongBanKhoa { get; set; }
        public decimal Gia { get; set; }
        public string TrangThaiTB { get; set; }
        public decimal TongChiPhiSuaChua { get; set; }
        public decimal KhauHao { get { return Gia * 0.2m; } }
        public decimal GiaTriConLai { get { return Gia - KhauHao; } }
    }
}
