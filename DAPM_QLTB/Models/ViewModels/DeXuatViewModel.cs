using System;
using System.Collections.Generic;

namespace QLTB.Models
{
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

    public class HoatDongGanDayViewModel
    {
        public string   ID_DeXuat    { get; set; }
        public string   MoTa         { get; set; }
        public string   NguoiDeXuat  { get; set; }
        public string   KhoaPhongBan { get; set; }
        public DateTime NgayDeXuat   { get; set; }
        public string   TrangThai    { get; set; }
    }
}
