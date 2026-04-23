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
        public string   MoTa            { get; set; }
        public string   LyDoTuChoi      { get; set; }
        public decimal  TongGiaDuKien   { get; set; }
        public bool     DaNhapThietBi   { get; set; }
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
    // ── Báo hỏng thiết bị ────────────────────────────────────
    public class BaoHongViewModel
    {
        public string ID_BaoHong { get; set; }
        public string ThietBiNo { get; set; }
        public string TenTB { get; set; }
        public string TenDanhMuc { get; set; }
        public string KhoaPhongBan { get; set; }
        public string NguoiBaoHongNo { get; set; }
        public string HoTenNguoiBao { get; set; }
        public string MoTaHong { get; set; }
        public DateTime NgayBao { get; set; }
        public string MucDoUuTien { get; set; }
        public string TrangThai { get; set; }
        // Kế hoạch đã được tạo từ báo hỏng này (nếu có)
        public string KeHoachNo { get; set; }
    }

    // ── Thiết bị (dùng cho dropdown) ─────────────────────────
    public class ThietBiDropdownViewModel
    {
        public string ID_ThietBi { get; set; }
        public string TenTB { get; set; }
        public string TrangThai { get; set; }
        public string DanhMuc { get; set; }
        public bool DangBaoHong { get; set; }
        public string BaoHongNo { get; set; }
        public string MoTaBaoHong { get; set; }
        public string MucDoUuTienBaoHong { get; set; }
    }

    // ── Chi tiết kế hoạch (1 thiết bị trong 1 kế hoạch) ─────
    public class ChiTietKeHoachViewModel
    {
        public string ID_ChiTietKH { get; set; }
        public string KeHoachNo { get; set; }
        public string ThietBiNo { get; set; }
        public string TenTB { get; set; }
        public string TenDanhMuc { get; set; }
        public string BaoHongNo { get; set; }   // null = định kỳ
        public string MoTaBaoHong { get; set; }
        public string NguonGoc { get; set; }   // "Định kỳ" | "Báo hỏng"
        public string GhiChuChiTiet { get; set; }
    }

    // ── Kế hoạch bảo trì (đầy đủ) ───────────────────────────
    public class KeHoachViewModel
    {
        public string ID_KeHoach { get; set; }
        public string NguoiLapNo { get; set; }
        public string HoTenNguoiLap { get; set; }
        public DateTime NgayLap { get; set; }
        public DateTime? NgayDuKienHT { get; set; }
        public string LoaiKeHoach { get; set; }   // "Sửa chữa" | "Định kỳ"
        public string DonViThucHien { get; set; }
        public decimal? ChiPhiDuKien { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
        public List<ChiTietKeHoachViewModel> ChiTiet { get; set; }

        public KeHoachViewModel() { ChiTiet = new List<ChiTietKeHoachViewModel>(); }
    }

    // ── Ghi nhận sửa chữa (đầy đủ) ──────────────────────────
    public class GhiNhanViewModel
    {
        public string ID_GhiNhan { get; set; }
        public string KeHoachNo { get; set; }
        public string LoaiKeHoach { get; set; }
        public string ChiTietKeHoachNo { get; set; }
        public string ThietBiNo { get; set; }
        public string TenTB { get; set; }
        public string BaoHongNo { get; set; }
        public string MoTaBaoHong { get; set; }
        public string DonViThucHien { get; set; }
        public DateTime NgayThucHien { get; set; }
        public string KetQua { get; set; }
        public decimal? ChiPhiThucTe { get; set; }
        public string TrangThaiSauSua { get; set; }
    }

    // ── ViewModel tổng hợp cho trang LapKeHoachBaoTri ────────
    public class LapKeHoachPageViewModel
    {
        public List<BaoHongViewModel> BaoHongChoCho { get; set; }  // chờ xử lý
        public List<KeHoachViewModel> DanhSachKeHoach { get; set; }
        public List<ThietBiDropdownViewModel> DanhSachThietBi { get; set; }

        public LapKeHoachPageViewModel()
        {
            BaoHongChoCho = new List<BaoHongViewModel>();
            DanhSachKeHoach = new List<KeHoachViewModel>();
            DanhSachThietBi = new List<ThietBiDropdownViewModel>();
        }
    }

    // ── ViewModel tổng hợp cho trang GhiNhanSuaChua ──────────
    public class GhiNhanPageViewModel
    {
        public List<KeHoachViewModel> KeHoachChoCho { get; set; }  // đang/chờ thực hiện
        public List<GhiNhanViewModel> LichSuGhiNhan { get; set; }

        public GhiNhanPageViewModel()
        {
            KeHoachChoCho = new List<KeHoachViewModel>();
            LichSuGhiNhan = new List<GhiNhanViewModel>();
        }
    }
}

// ViewModels kiểm kê — thêm vào namespace chính
namespace QLTB.Models
{
    using System;
    using System.Collections.Generic;

    // ── Kiểm kê tài sản ──────────────────────────────────────
    public class ThietBiKiemKeRow
    {
        public string    ID_ThietBi      { get; set; }
        public string    TenTB           { get; set; }
        public string    TrangThaiTB     { get; set; }
        public string    TenKhoa         { get; set; }
        public string    TenDanhMuc      { get; set; }
        public int       SoLuongHeThong  { get; set; }  // SL sổ sách, mặc định 1
        public int?      SoLuongThucTe   { get; set; }
        public string    TinhTrangThucTe { get; set; }
        public string    GhiChu          { get; set; }
        public DateTime? NgayKiemKe      { get; set; }
    }

    public class KiemKeTaiSanViewModel
    {
        public List<ThietBiKiemKeRow> DanhSachThietBi { get; set; }
        public int TongThietBi { get; set; }
        public int DaKiemKe    { get; set; }
        public int ChuaKiemKe  { get; set; }

        public KiemKeTaiSanViewModel()
        {
            DanhSachThietBi = new List<ThietBiKiemKeRow>();
        }
    }

    public class ItemTaoKiemKe
    {
        public string ID_ThietBi      { get; set; }
        public string TenTB           { get; set; }
        public string TrangThaiTB     { get; set; }
        public string TenKhoa         { get; set; }
        public string TenDanhMuc      { get; set; }
        public int    SoLuongHeThong  { get; set; }  // SL sổ sách, mặc định 1
        public int?   SoLuongThucTe   { get; set; }
        public string TinhTrangThucTe { get; set; }
        public string GhiChu          { get; set; }
    }

    public class TaoPhieuKiemKeViewModel
    {
        public string   ID_KiemKe    { get; set; }
        public DateTime NgayKiemKe   { get; set; }
        public string   NguoiTao     { get; set; }
        public List<ItemTaoKiemKe> DanhSachChuaKiem { get; set; }

        public TaoPhieuKiemKeViewModel()
        {
            DanhSachChuaKiem = new List<ItemTaoKiemKe>();
        }
    }
}
