using System;
using System.Collections.Generic;

namespace QLTB.Models
{
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
