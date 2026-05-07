using System.Collections.Generic;
using System.Data.SqlClient;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class DeXuatService
    {
        private readonly DeXuatRepository _repo = new DeXuatRepository();

        /// <summary>Trưởng Khoa gửi đề xuất + gửi thông báo cho CSVC.</summary>
        public (bool ok, string msg, string idDX, int demTB) GuiDeXuat(
            string userId, string mota, string[] tenTB, int[] soluong, decimal[] gia, string[] donvi)
        {
            var result = _repo.GuiDeXuat(userId, mota, tenTB, soluong, gia, donvi);

            // Gửi thông báo cho CSVC sau khi commit (không ảnh hưởng đề xuất nếu lỗi)
            if (result.ok)
            {
                try
                {
                    using (var conn = DbHelper.GetConnection())
                    {
                        conn.Open();
                        NotificationHelper.GuiTheoVaiTro(conn, null,
                            "VT_CSVC",
                            "📋 Đề xuất mua sắm mới cần xét duyệt",
                            "Trưởng khoa đã gửi đề xuất " + result.demTB + " thiết bị (Mã: " + result.idDX + "). Vui lòng xem xét và phê duyệt.",
                            "pending");
                    }
                }
                catch { /* Thông báo lỗi không ảnh hưởng đề xuất */ }
            }

            return result;
        }

        public (List<DeXuatViewModel> choDuyet, List<DeXuatViewModel> lichSu) GetDeXuatForCSVC()
            => _repo.GetDeXuatForCSVC();

        public (bool ok, string msg) XuLyDeXuat(string id, string action, string ghiChu, string nguoiDuyetId)
            => _repo.XuLyDeXuat(id, action, ghiChu, nguoiDuyetId);

        public List<object> GetChiTiet(string id) => _repo.GetChiTiet(id);

        public (bool ok, string msg) HoanTatNhapThietBi(string id, string nguoiDuyetId)
            => _repo.HoanTatNhapThietBi(id, nguoiDuyetId);

        public List<object> GetLichSuDuyet(string idDX) => _repo.GetLichSuDuyet(idDX);
        public List<object> GetDanhSachDeXuat() => _repo.GetDanhSachDeXuat();
    }
}
