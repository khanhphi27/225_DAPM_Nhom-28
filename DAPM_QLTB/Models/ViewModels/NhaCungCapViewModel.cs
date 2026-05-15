namespace QLTB.Models
{
    // ── Quản lý nhà cung cấp ────────────────────────────────
    public class NhaCungCapViewModel
    {
        public string ID_NhaCC   { get; set; }
        public string TenNhaCC   { get; set; }
        public int?   LoaiDichVu { get; set; }
        public string DiaChi     { get; set; }
        public string Sdt        { get; set; }
        public int    SoThietBi  { get; set; }
    }
}
