namespace QLTB.Models
{
    // ── Quản lý người dùng ───────────────────────────────────
    public class NguoiDungViewModel
    {
        public string ID_NguoiDung { get; set; }
        public string HoTen        { get; set; }
        public string Email        { get; set; }
        public bool   TrangThaiTK  { get; set; }
        public string TenKhoa      { get; set; }
        public string VaiTroNo     { get; set; }
        public string TenVaiTro    { get; set; }
    }
}
