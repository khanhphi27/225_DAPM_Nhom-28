using System;
using System.Data.SqlClient;

namespace QLTB.Models
{
    /// <summary>
    /// Helper gửi thông báo vào bảng THONGBAO cho một hoặc nhiều người dùng.
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>Gửi thông báo cho tất cả người dùng có vai trò chỉ định.</summary>
        /// <param name="conn">Kết nối SQL đang mở (có thể dùng chung transaction).</param>
        /// <param name="tran">Transaction hiện tại (null nếu không dùng).</param>
        /// <param name="vaiTroId">VT_TK | VT_CSVC | VT_KHTC | VT_BGH</param>
        /// <param name="tieuDe">Tiêu đề thông báo.</param>
        /// <param name="noiDung">Nội dung chi tiết.</param>
        /// <param name="loai">Loại: approved | rejected | pending | system</param>
        public static void GuiTheoVaiTro(SqlConnection conn, SqlTransaction tran,
            string vaiTroId, string tieuDe, string noiDung, string loai = "system")
        {
            const string sql = @"
                INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
                SELECT NEWID(), vn.NguoiDungNo, @TieuDe, @NoiDung, GETDATE(), @Loai, 0
                FROM VAITRO_NGUOIDUNG vn
                WHERE vn.VaiTroNo = @VaiTroId";

            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@VaiTroId", vaiTroId);
                cmd.Parameters.AddWithValue("@TieuDe",   tieuDe);
                cmd.Parameters.AddWithValue("@NoiDung",  noiDung);
                cmd.Parameters.AddWithValue("@Loai",     loai);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Gửi thông báo cho một người dùng cụ thể.</summary>
        public static void GuiChoNguoiDung(SqlConnection conn, SqlTransaction tran,
            string nguoiDungId, string tieuDe, string noiDung, string loai = "system")
        {
            const string sql = @"
                INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
                VALUES (NEWID(), @NguoiDung, @TieuDe, @NoiDung, GETDATE(), @Loai, 0)";

            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@NguoiDung", nguoiDungId);
                cmd.Parameters.AddWithValue("@TieuDe",    tieuDe);
                cmd.Parameters.AddWithValue("@NoiDung",   noiDung);
                cmd.Parameters.AddWithValue("@Loai",      loai);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
