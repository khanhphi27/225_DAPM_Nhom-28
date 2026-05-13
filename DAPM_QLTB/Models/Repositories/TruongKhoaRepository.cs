using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace QLTB.Models.Repositories
{
    public class TruongKhoaRepository
    {
        #region ── Helper ──────────────────────────────────────

        /// <summary>Lấy Khoa/PhòngBan của user hiện tại.</summary>
        public (string khoaBanNo, string tenKhoa) GetKhoaPhongBanByUser(string userId)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT ISNULL(u.Khoa_BanNo,''), ISNULL(kp.TenPhongBanKhoa,'')
                    FROM NGUOIDUNG u LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=u.Khoa_BanNo
                    WHERE u.ID_NguoiDung=@UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) return (r[0]?.ToString() ?? "", r[1]?.ToString() ?? "");
                }
            }
            return ("", "");
        }

        #endregion

        #region ── Đề xuất mua sắm ────────────────────────────

        /// <summary>Lấy danh sách đề xuất của user (DataTable cho View).</summary>
        public DataTable GetDeXuatByUser(string userId)
        {
            var dt = new DataTable();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT d.ID_DeXuat, d.NgayDeXuat, d.TrangThai, d.MoTa, d.LyDoTuChoi, d.NgayDuyetCuoi,
                           c.TenThietBiDeXuat, c.SoLuong, c.GiaDuKien, c.DonViTinh
                    FROM   DEXUAT_MUASAM d
                    JOIN   CHITIET_DEXUAT c ON c.DeXuatNo = d.ID_DeXuat
                    WHERE  d.NguoiDeXuatNo = @UserId
                    ORDER  BY d.NgayDeXuat DESC";
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@UserId", userId);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>Gửi đề xuất mua sắm mới.</summary>
        public (bool ok, string msg, string idDX, int dem) GuiDeXuat(
            string userId, string mota, string[] tenTB, int[] soluong, decimal[] gia, string[] donvi)
        {
            string idDX = null;
            int dem = 0;
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmdId = new SqlCommand("SELECT LEFT(REPLACE(NEWID(),'-',''),10)", conn, tran))
                            idDX = cmdId.ExecuteScalar().ToString();

                        using (var cmd = new SqlCommand(@"INSERT INTO DEXUAT_MUASAM (ID_DeXuat,NguoiDeXuatNo,NgayDeXuat,TrangThai,MoTa)
                            VALUES (@id,@user,GETDATE(),N'Chờ CSVC duyệt',@mota)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", idDX);
                            cmd.Parameters.AddWithValue("@user", userId);
                            cmd.Parameters.AddWithValue("@mota", (object)mota ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        for (int i = 0; i < tenTB.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(tenTB[i])) continue;
                            using (var cmd = new SqlCommand(@"INSERT INTO CHITIET_DEXUAT (ID_ChiTiet,DeXuatNo,TenThietBiDeXuat,SoLuong,GiaDuKien,DonViTinh)
                                VALUES (LEFT(REPLACE(NEWID(),'-',''),10),@dx,@ten,@sl,@gia,@dvt)", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@dx", idDX);
                                cmd.Parameters.AddWithValue("@ten", tenTB[i]);
                                cmd.Parameters.AddWithValue("@sl", soluong != null && i < soluong.Length ? soluong[i] : 1);
                                cmd.Parameters.AddWithValue("@gia", gia != null && i < gia.Length ? gia[i] : 0m);
                                cmd.Parameters.AddWithValue("@dvt", donvi != null && i < donvi.Length && !string.IsNullOrEmpty(donvi[i]) ? (object)donvi[i] : DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                            dem++;
                        }
                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
            return (true, "Gửi đề xuất thành công! " + dem + " thiết bị đang chờ Phòng CSVC xét duyệt.", idDX, dem);
        }

        /// <summary>Chỉnh sửa và gửi lại đề xuất (reset về Chờ CSVC duyệt).</summary>
        public (bool ok, string msg) ChinhSuaDeXuat(
            string idDX, string userId, string mota, string[] tenTB, int[] soluong, decimal[] gia, string[] donvi)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Kiểm tra quyền
                        string trangThaiHienTai = null;
                        using (var cmd = new SqlCommand("SELECT TrangThai FROM DEXUAT_MUASAM WHERE ID_DeXuat=@Id AND NguoiDeXuatNo=@User", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", idDX);
                            cmd.Parameters.AddWithValue("@User", userId);
                            var val = cmd.ExecuteScalar();
                            if (val != null) trangThaiHienTai = val.ToString();
                        }

                        if (trangThaiHienTai == null) return (false, "Không tìm thấy phiếu hoặc bạn không có quyền chỉnh sửa.");
                        if (trangThaiHienTai == "Đã duyệt") return (false, "Phiếu đã được BGH phê duyệt hoàn tất, không thể chỉnh sửa.");

                        string resetMsg;
                        if (trangThaiHienTai == "Chờ CSVC duyệt" || trangThaiHienTai.Contains("Từ chối"))
                            resetMsg = "Đã cập nhật và gửi lại! Đang chờ Phòng CSVC xét duyệt.";
                        else if (trangThaiHienTai == "Chờ KHTC duyệt")
                            resetMsg = "Đã cập nhật! Phiếu reset về Chờ CSVC duyệt — CSVC sẽ duyệt lại từ đầu.";
                        else
                            resetMsg = "Đã cập nhật! Phiếu reset về Chờ CSVC duyệt — tất cả các bên sẽ duyệt lại từ đầu.";

                        // Reset trạng thái
                        using (var cmd = new SqlCommand(@"UPDATE DEXUAT_MUASAM
                            SET TrangThai=N'Chờ CSVC duyệt', MoTa=@MoTa, NgayDeXuat=GETDATE(),
                                LyDoTuChoi=NULL, NgayDuyetCuoi=NULL, NguoiDuyetCuoiNo=NULL
                            WHERE ID_DeXuat=@Id", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@MoTa", (object)mota ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Id", idDX);
                            cmd.ExecuteNonQuery();
                        }

                        // Xóa chi tiết cũ
                        using (var cmd = new SqlCommand("DELETE FROM CHITIET_DEXUAT WHERE DeXuatNo=@Id", conn, tran))
                        { cmd.Parameters.AddWithValue("@Id", idDX); cmd.ExecuteNonQuery(); }

                        // Insert chi tiết mới
                        for (int i = 0; i < tenTB.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(tenTB[i])) continue;
                            using (var cmd = new SqlCommand(@"INSERT INTO CHITIET_DEXUAT (ID_ChiTiet,DeXuatNo,TenThietBiDeXuat,SoLuong,GiaDuKien,DonViTinh)
                                VALUES (LEFT(REPLACE(NEWID(),'-',''),10),@dx,@ten,@sl,@gia,@dvt)", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@dx", idDX);
                                cmd.Parameters.AddWithValue("@ten", tenTB[i]);
                                cmd.Parameters.AddWithValue("@sl", soluong != null && i < soluong.Length ? soluong[i] : 1);
                                cmd.Parameters.AddWithValue("@gia", gia != null && i < gia.Length ? gia[i] : 0m);
                                cmd.Parameters.AddWithValue("@dvt", donvi != null && i < donvi.Length && !string.IsNullOrEmpty(donvi[i]) ? (object)donvi[i] : DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                        return (true, resetMsg);
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
        }

        #endregion

        #region ── Danh sách thiết bị ──────────────────────────

        /// <summary>Lấy danh sách thiết bị theo khoa (DataTable cho View).</summary>
        public DataTable GetThietBiByKhoa(string khoaBanNo)
        {
            var dt = new DataTable();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT RTRIM(tb.ID_ThietBi) AS ID_ThietBi, tb.TenTB, tb.Gia,
                           ISNULL(tb.ThongSoKT,'') AS ThongSoKT, ISNULL(tb.TrangThaiTB,'') AS TrangThaiTB,
                           ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                           ISNULL(kp.TenPhongBanKhoa,'') AS TenKhoaPhongBan,
                           ISNULL(ncc.TenNhaCC,'') AS TenNhaCC,
                           ISNULL(CONVERT(varchar(20), tb.SoSeri), '') AS SoSeri
                    FROM THIETBI tb
                    LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                    LEFT JOIN NHACUNGCAP ncc ON ncc.ID_NhaCC=tb.NhaCCNo
                    WHERE tb.KhoaPhongBan=@KhoaPhongBan ORDER BY tb.TenTB";
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@KhoaPhongBan", khoaBanNo);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>Báo hỏng thiết bị.</summary>
        public (bool ok, string msg) BaoHong(string idThietBi, string mota, string userId, string khoaBanNo)
        {
            var tbId = (idThietBi ?? "").Trim();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();

                // Kiểm tra TB thuộc khoa
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM THIETBI WHERE RTRIM(ID_ThietBi)=@tb AND KhoaPhongBan=@khoa", conn))
                {
                    cmd.Parameters.AddWithValue("@tb", tbId);
                    cmd.Parameters.AddWithValue("@khoa", khoaBanNo);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                        return (false, "Bạn chỉ được báo hỏng thiết bị thuộc khoa/phòng ban của mình.");
                }

                // Lấy tên TB
                string tenTB = tbId;
                using (var cmd = new SqlCommand("SELECT TenTB FROM THIETBI WHERE RTRIM(ID_ThietBi)=@tb", conn))
                {
                    cmd.Parameters.AddWithValue("@tb", tbId);
                    var val = cmd.ExecuteScalar();
                    if (val != null) tenTB = val.ToString();
                }

                string idBaoHong = "BH" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"INSERT INTO BAOHONG_THIETBI (ID_BaoHong,ThietBiNo,NguoiBaoHongNo,MoTaHong,NgayBao,MucDoUuTien,TrangThai)
                            VALUES (@id,@tb,@user,@mota,GETDATE(),N'Cao',N'Chờ xử lý')", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", idBaoHong);
                            cmd.Parameters.AddWithValue("@tb", tbId);
                            cmd.Parameters.AddWithValue("@user", userId);
                            cmd.Parameters.AddWithValue("@mota", mota ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand("UPDATE THIETBI SET TrangThaiTB=N'Báo hỏng' WHERE RTRIM(ID_ThietBi)=@tb", conn, tran))
                        { cmd.Parameters.AddWithValue("@tb", tbId); cmd.ExecuteNonQuery(); }

                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }

                // Gửi thông báo sau commit
                try { NotificationHelper.GuiTheoVaiTro(conn, null, "VT_CSVC", "🔧 Báo hỏng thiết bị: " + tenTB + " (" + idBaoHong + ")", mota ?? "", "pending"); }
                catch { }
            }
            return (true, "Đã gửi báo cáo hỏng!");
        }

        #endregion
    }
}
