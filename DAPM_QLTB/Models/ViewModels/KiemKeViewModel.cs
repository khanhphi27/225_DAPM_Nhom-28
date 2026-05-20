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

    /// <summary>Nhóm thiết bị cùng tên trong 1 khoa/phòng ban (dùng cho DanhSach)</summary>
    public class NhomThietBiKiemKe
    {
        public string TenTB          { get; set; }
        public string TenDanhMuc     { get; set; }
        public string TenKhoa        { get; set; }
        public int    SoLuongSoSach  { get; set; }   // tổng SL sổ sách
        public int?   SoLuongThucTe  { get; set; }   // tổng SL thực tế (null nếu chưa kiểm)
        public string KetQuaKiemKe   { get; set; }   // "Đủ" / "Thiếu" / "Chưa kiểm"
        public string GhiChu         { get; set; }   // ghi chú tổng hợp
        public List<ThietBiKiemKeRow> ChiTiet { get; set; } = new List<ThietBiKiemKeRow>();
    }

    /// <summary>Nhóm theo khoa/phòng ban (dùng cho DanhSach)</summary>
    public class NhomKhoaKiemKe
    {
        public string TenKhoa { get; set; }
        public List<NhomThietBiKiemKe> NhomThietBi { get; set; } = new List<NhomThietBiKiemKe>();
        public int TongThietBi  => NhomThietBi.Count;
        public int DaKiemKe     => NhomThietBi.FindAll(x => x.KetQuaKiemKe != "Chưa kiểm").Count;
    }

    public class KiemKeTaiSanViewModel
    {
        public List<ThietBiKiemKeRow> DanhSachThietBi { get; set; }
        public List<NhomKhoaKiemKe>   NhomTheoKhoa    { get; set; }
        public int TongThietBi { get; set; }
        public int DaKiemKe    { get; set; }
        public int ChuaKiemKe  { get; set; }

        public KiemKeTaiSanViewModel()
        {
            DanhSachThietBi = new List<ThietBiKiemKeRow>();
            NhomTheoKhoa    = new List<NhomKhoaKiemKe>();
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

    /// <summary>Nhóm thiết bị cùng tên trong 1 khoa/phòng ban (dùng cho TaoPhieu)</summary>
    public class NhomThietBiTaoPhieu
    {
        public string TenTB          { get; set; }
        public string TenDanhMuc     { get; set; }
        public string TenKhoa        { get; set; }
        public int    SoLuongSoSach  { get; set; }   // tổng số TB cùng tên
        public int    SoLuongThucTe  { get; set; }   // số TB không hỏng
        public List<ItemTaoKiemKe> ChiTiet { get; set; } = new List<ItemTaoKiemKe>();
    }

    /// <summary>Nhóm theo khoa/phòng ban (dùng cho TaoPhieu)</summary>
    public class NhomKhoaTaoPhieu
    {
        public string TenKhoa { get; set; }
        public List<NhomThietBiTaoPhieu> NhomThietBi { get; set; } = new List<NhomThietBiTaoPhieu>();
    }

    public class TaoPhieuKiemKeViewModel
    {
        public string   ID_KiemKe    { get; set; }
        public DateTime NgayKiemKe   { get; set; }
        public string   NguoiTao     { get; set; }
        public List<ItemTaoKiemKe>      DanhSachChuaKiem { get; set; }
        public List<NhomKhoaTaoPhieu>   NhomTheoKhoa     { get; set; }

        public TaoPhieuKiemKeViewModel()
        {
            DanhSachChuaKiem = new List<ItemTaoKiemKe>();
            NhomTheoKhoa     = new List<NhomKhoaTaoPhieu>();
        }
    }
}
