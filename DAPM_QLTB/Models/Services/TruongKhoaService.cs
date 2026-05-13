using System.Data;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class TruongKhoaService
    {
        private readonly TruongKhoaRepository _repo = new TruongKhoaRepository();

        public (string khoaBanNo, string tenKhoa) GetKhoaPhongBanByUser(string userId)
            => _repo.GetKhoaPhongBanByUser(userId);

        public DataTable GetDeXuatByUser(string userId) => _repo.GetDeXuatByUser(userId);

        public (bool ok, string msg, string idDX, int dem) GuiDeXuat(
            string userId, string mota, string[] tenTB, int[] soluong, decimal[] gia, string[] donvi)
        {
            var result = _repo.GuiDeXuat(userId, mota, tenTB, soluong, gia, donvi);
            if (result.ok)
            {
                try
                {
                    using (var conn = DbHelper.GetConnection())
                    {
                        conn.Open();
                        NotificationHelper.GuiTheoVaiTro(conn, null, "VT_CSVC",
                            "📋 Đề xuất mua sắm mới cần xét duyệt",
                            "Trưởng khoa đã gửi đề xuất " + result.dem + " thiết bị (Mã: " + result.idDX + "). Vui lòng xem xét và phê duyệt.",
                            "pending");
                    }
                }
                catch { }
            }
            return result;
        }

        public (bool ok, string msg) ChinhSuaDeXuat(
            string idDX, string userId, string mota, string[] tenTB, int[] soluong, decimal[] gia, string[] donvi)
        {
            var result = _repo.ChinhSuaDeXuat(idDX, userId, mota, tenTB, soluong, gia, donvi);
            if (result.ok)
            {
                try
                {
                    using (var conn = DbHelper.GetConnection())
                    {
                        conn.Open();
                        NotificationHelper.GuiTheoVaiTro(conn, null, "VT_CSVC",
                            "📋 Đề xuất mua sắm đã được cập nhật",
                            "Trưởng khoa đã chỉnh sửa và gửi lại đề xuất (Mã: " + idDX + "). Vui lòng xem xét lại.",
                            "pending");
                    }
                }
                catch { }
            }
            return result;
        }

        public DataTable GetThietBiByKhoa(string khoaBanNo) => _repo.GetThietBiByKhoa(khoaBanNo);

        public (bool ok, string msg) BaoHong(string idThietBi, string mota, string userId, string khoaBanNo)
            => _repo.BaoHong(idThietBi, mota, userId, khoaBanNo);
    }
}
