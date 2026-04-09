using System;
using System.Collections.Generic;

namespace QLTB.Models
{
    // ── Thông báo ────────────────────────────────────────────
    public class ThongBaoViewModel
    {
        public string   ID_ThongBao  { get; set; }
        public string   TieuDe       { get; set; }
        public string   NoiDung      { get; set; }
        public DateTime NgayTao      { get; set; }
        public string   LoaiThongBao { get; set; }
        public bool     DaDoc        { get; set; }
        public string   NguoiTao     { get; set; }
    }

    // ── BGH Dashboard ────────────────────────────────────────
    public class BGHDashboardViewModel
    {
        public int TongThietBi    { get; set; }
        public int HoatDong       { get; set; }
        public int BaoTri         { get; set; }
        public int Hong           { get; set; }
        public int ChoDeXuatDuyet { get; set; }
        public List<HoatDongGanDayViewModel> HoatDongGanDay { get; set; }
    }

    public class HoatDongGanDayViewModel
    {
        public string   ID_DeXuat    { get; set; }
        public string   MoTa         { get; set; }
        public string   NguoiDeXuat  { get; set; }
        public string   KhoaPhongBan { get; set; }
        public DateTime NgayDeXuat   { get; set; }
        public string   TrangThai    { get; set; }
    }

    // ── Đề xuất mua sắm ──────────────────────────────────────
    public class DeXuatViewModel
    {
        public string   ID_DeXuat       { get; set; }
        public string   NguoiDeXuat     { get; set; }
        public string   KhoaPhongBan    { get; set; }
        public DateTime NgayDeXuat      { get; set; }
        public string   TrangThai       { get; set; }
        public int?     CapDuyetHienTai { get; set; }
        public string   MoTa            { get; set; }
        public string   LyDoTuChoi      { get; set; }
        public decimal  TongGiaDuKien   { get; set; }
        public List<ChiTietDeXuatViewModel> ChiTiet { get; set; }
    }

    public class ChiTietDeXuatViewModel
    {
        public string   TenThietBiDeXuat { get; set; }
        public int      SoLuong          { get; set; }
        public decimal? GiaDuKien        { get; set; }
        public string   DonViTinh        { get; set; }
        public string   TenDanhMuc       { get; set; }
    }

    // ── BGH Thống kê tài sản ─────────────────────────────────
    public class ThongKeTaiSanViewModel
    {
        public int     TongThietBi { get; set; }
        public int     HoatDong    { get; set; }
        public int     BaoTri      { get; set; }
        public int     Hong        { get; set; }
        public decimal TongGiaTri  { get; set; }
        public List<ThongKeTheoKhoaViewModel>    TheoKhoa     { get; set; }
        public List<ThongKeTheoDanhMucViewModel> TheoDanhMuc  { get; set; }
        public List<KiemKeViewModel>             LichSuKiemKe { get; set; }
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

    public class KiemKeViewModel
    {
        public string   ID_KiemKe     { get; set; }
        public DateTime NgayKiemKe    { get; set; }
        public string   NguoiThucHien { get; set; }
        public string   TrangThai     { get; set; }
        public string   GhiChu        { get; set; }
        public int      TongThietBiKK { get; set; }
    }

    // ── BGH Theo dõi tình trạng ──────────────────────────────
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

    // ── KHTC Dashboard ───────────────────────────────────────
    public class KHTCDashboardViewModel
    {
        public decimal TongGiaTri  { get; set; }
        public decimal TongSuaChua { get; set; }
        public int     ChoDuyet    { get; set; }
        public List<DeXuatViewModel> HoatDongGanDay { get; set; }

        public KHTCDashboardViewModel()
        {
            HoatDongGanDay = new List<DeXuatViewModel>();
        }
    }

    // ── KHTC Chi phí & Báo cáo ───────────────────────────────
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
        public string  ID_ThietBi        { get; set; }
        public string  TenTB             { get; set; }
        public string  TenDanhMuc        { get; set; }
        public string  TenPhongBanKhoa   { get; set; }
        public decimal Gia               { get; set; }
        public string  TrangThaiTB       { get; set; }
        public decimal TongChiPhiSuaChua { get; set; }
    }
}
