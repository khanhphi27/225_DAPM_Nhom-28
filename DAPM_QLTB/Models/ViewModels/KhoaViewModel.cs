namespace QLTB.Models
{
    // ── Quản lý khoa / phòng ban ─────────────────────────────
    public class KhoaPhongBanViewModel
    {
        public string ID_KhoaPhongBan { get; set; }
        public string TenPhongBanKhoa { get; set; }
        public int    SoNguoiDung     { get; set; }
        public int    SoThietBi       { get; set; }
    }
}
