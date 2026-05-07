using System;

namespace QLTB.Models
{
    public class ThietBiViewModel
    {
        public string ID_ThietBi { get; set; }
        public string TenTB { get; set; }
        public string DanhMuc { get; set; }
        public string KhoaPhongBan { get; set; }
        public string PhongCoSoNo { get; set; }
        public string PhongKhuVucNo { get; set; }
        public string PhongNo { get; set; }
        public string NhaCungCap { get; set; }
        public int? SoSeri { get; set; }
        public decimal? Gia { get; set; }
        public string TrangThaiTB { get; set; }
        public string DeXuatNo { get; set; }
    }
}