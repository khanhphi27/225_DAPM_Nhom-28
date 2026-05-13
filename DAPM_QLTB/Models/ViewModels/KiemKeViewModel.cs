using System;
using System.Collections.Generic;

namespace QLTB.Models
{
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
