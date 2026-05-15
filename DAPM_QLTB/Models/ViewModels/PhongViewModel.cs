namespace QLTB.Models
{
    // ── Quản lý phòng ────────────────────────────────────────
    public class PhongViewModel
    {
        public string ID_Phong  { get; set; }
        public string TenPhong  { get; set; }
        public string CoSoNo    { get; set; }
        public string KhuVucNo  { get; set; }
        public string TenKhuVuc { get; set; }
        public int?   SucChua   { get; set; }
        public int    SoThietBi { get; set; }
    }

    public class KhuViewModel
    {
        public string ID_KhuVuc { get; set; }
        public string TenKhuVuc { get; set; }
        public string CoSoNo    { get; set; }
        public string TenCoSo   { get; set; }
    }
}
