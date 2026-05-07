using System;

namespace QLTB.Models
{
    public class ThietBiViewModel
    {
        public string ID_ThietBi { get; set; }
        public string TenTB { get; set; }
        public string DanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public string KhoaPhongBan { get; set; }
        public string TenKhoaPhongBan { get; set; }
        public string PhongCoSoNo { get; set; }
        public string TenCoSo { get; set; }
        public string PhongKhuVucNo { get; set; }
        public string TenKhuVuc { get; set; }
        public string PhongNo { get; set; }
        public string TenPhong { get; set; }
        public string NhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public int? SoSeri { get; set; }
        public decimal? Gia { get; set; }
        public string TrangThaiTB { get; set; }
        public string DeXuatNo { get; set; }
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
}
