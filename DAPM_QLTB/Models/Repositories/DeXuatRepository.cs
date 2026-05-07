using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace QLTB.Models.Repositories
{
    public class DeXuatRepository
    {
        #region ── Trưởng Khoa: Gửi đề xuất ───────────────────

        /// <summary>Tạo đề xuất mua sắm mới (TruongKhoa).</summary>
        public (bool ok, string msg, string idDX, int demTB) GuiDeXuat(
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

        #endregion

        #region ── CSVC: Phê duyệt đề xuất ────────────────────

        /// <summary>Danh sách đề xuất cho CSVC phê duyệt.</summary>
        public (List<DeXuatViewModel> choDuyet, List<DeXuatViewModel> lichSu) GetDeXuatForCSVC()
        {
            var choDuyet = new List<DeXuatViewModel>();
            var lichSu = new List<DeXuatViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT dx.ID_DeXuat,dx.NgayDeXuat,dx.TrangThai,dx.MoTa,dx.LyDoTuChoi,
                           nd.HoTen AS NguoiDeXuat, ISNULL(kp.TenPhongBanKhoa,N'') AS KhoaPhongBan,
                           ISNULL((SELECT SUM(ct.SoLuong*ISNULL(ct.GiaDuKien,0)) FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo=dx.ID_DeXuat),0) AS TongGia,
                           CASE WHEN EXISTS (SELECT 1 FROM LICHSUDUYET ls WHERE ls.DeXuatNo=dx.ID_DeXuat AND LTRIM(RTRIM(ls.TrangThaiSauDuyet))=N'Đã nhập thiết bị') THEN 1 ELSE 0 END AS DaNhapThietBi
                    FROM DEXUAT_MUASAM dx JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=dx.NguoiDeXuatNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=nd.Khoa_BanNo
                    ORDER BY dx.NgayDeXuat DESC";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                    {
                        var item = new DeXuatViewModel
                        {
                            ID_DeXuat = rd["ID_DeXuat"].ToString(),
                            NguoiDeXuat = rd["NguoiDeXuat"].ToString(),
                            KhoaPhongBan = rd["KhoaPhongBan"].ToString(),
                            NgayDeXuat = Convert.ToDateTime(rd["NgayDeXuat"]),
                            TrangThai = rd["TrangThai"].ToString(),
                            MoTa = rd["MoTa"] == DBNull.Value ? "" : rd["MoTa"].ToString(),
                            LyDoTuChoi = rd["LyDoTuChoi"] == DBNull.Value ? "" : rd["LyDoTuChoi"].ToString(),
                            TongGiaDuKien = Convert.ToDecimal(rd["TongGia"]),
                            DaNhapThietBi = Convert.ToInt32(rd["DaNhapThietBi"]) == 1
                        };
                        if (item.TrangThai == "Chờ CSVC duyệt") choDuyet.Add(item); else lichSu.Add(item);
                    }
            }
            return (choDuyet, lichSu);
        }

        /// <summary>CSVC duyệt/từ chối đề xuất.</summary>
        public (bool ok, string msg) XuLyDeXuat(string id, string action, string ghiChu, string nguoiDuyetId)
        {
            string trangThaiMoi = action == "duyet" ? "Chờ KHTC duyệt" : "CSVC Từ chối";
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(@"UPDATE DEXUAT_MUASAM SET TrangThai=@TT, LyDoTuChoi=CASE WHEN @Act='tuchoi' THEN @GC ELSE LyDoTuChoi END, NgayDuyetCuoi=GETDATE() WHERE ID_DeXuat=@Id", conn, tran))
                    { cmd.Parameters.AddWithValue("@TT", trangThaiMoi); cmd.Parameters.AddWithValue("@Act", action ?? ""); cmd.Parameters.AddWithValue("@GC", (object)ghiChu ?? DBNull.Value); cmd.Parameters.AddWithValue("@Id", id); cmd.ExecuteNonQuery(); }

                    using (var cmd = new SqlCommand(@"INSERT INTO LICHSUDUYET (ID_LichSu,DeXuatNo,CapDuyet,NguoiDuyetNo,ThoiGianDuyet,TrangThaiSauDuyet,GhiChu)
                        VALUES (LEFT(REPLACE(NEWID(),'-',''),10),@DX,1,@ND,GETDATE(),@TT,@GC)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@DX", id); cmd.Parameters.AddWithValue("@ND", nguoiDuyetId);
                        cmd.Parameters.AddWithValue("@TT", trangThaiMoi); cmd.Parameters.AddWithValue("@GC", (object)ghiChu ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    tran.Commit();
                }
            }
            return (true, "Xử lý thành công.");
        }

        /// <summary>Chi tiết thiết bị của 1 đề xuất.</summary>
        public List<object> GetChiTiet(string id)
        {
            var list = new List<object>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TenThietBiDeXuat,SoLuong,GiaDuKien,DonViTinh FROM CHITIET_DEXUAT WHERE DeXuatNo=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            list.Add(new
                            {
                                TenThietBiDeXuat = rd["TenThietBiDeXuat"].ToString(),
                                SoLuong = rd["SoLuong"] == DBNull.Value ? 0 : Convert.ToInt32(rd["SoLuong"]),
                                GiaDuKien = rd["GiaDuKien"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["GiaDuKien"]),
                                DonViTinh = rd["DonViTinh"] == DBNull.Value ? "" : rd["DonViTinh"].ToString()
                            });
                }
            }
            return list;
        }

        /// <summary>CSVC hoàn tất nhập thiết bị từ đề xuất.</summary>
        public (bool ok, string msg) HoanTatNhapThietBi(string id, string nguoiDuyetId)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Thiếu mã đề xuất.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                // Kiểm tra đã duyệt bởi BGH
                const string sqlCheck = @"SELECT COUNT(1) FROM DEXUAT_MUASAM dx
                    WHERE dx.ID_DeXuat=@dx AND LTRIM(RTRIM(dx.TrangThai))=N'Đã duyệt'
                    AND EXISTS (SELECT 1 FROM LICHSUDUYET ls JOIN VAITRO_NGUOIDUNG vn ON vn.NguoiDungNo=ls.NguoiDuyetNo AND vn.VaiTroNo=N'VT_BGH'
                                WHERE ls.DeXuatNo=dx.ID_DeXuat AND ls.ThoiGianDuyet=(SELECT MAX(ls2.ThoiGianDuyet) FROM LICHSUDUYET ls2 WHERE ls2.DeXuatNo=dx.ID_DeXuat)
                                AND ls.CapDuyet=4 AND LTRIM(RTRIM(ls.TrangThaiSauDuyet))=N'Đã duyệt')";
                using (var cmd = new SqlCommand(sqlCheck, conn))
                { cmd.Parameters.AddWithValue("@dx", id.Trim()); if (Convert.ToInt32(cmd.ExecuteScalar()) == 0) return (false, "Đề xuất chưa ở trạng thái đã duyệt hoàn tất bởi BGH."); }

                const string sqlDone = @"IF NOT EXISTS (SELECT 1 FROM LICHSUDUYET WHERE DeXuatNo=@DX AND LTRIM(RTRIM(TrangThaiSauDuyet))=N'Đã nhập thiết bị')
                    INSERT INTO LICHSUDUYET (ID_LichSu,DeXuatNo,CapDuyet,NguoiDuyetNo,ThoiGianDuyet,TrangThaiSauDuyet,GhiChu)
                    VALUES (LEFT(REPLACE(NEWID(),'-',''),10),@DX,1,@ND,GETDATE(),N'Đã nhập thiết bị',N'CSVC hoàn tất nhập thiết bị từ đề xuất.');";
                using (var cmd = new SqlCommand(sqlDone, conn))
                { cmd.Parameters.AddWithValue("@DX", id.Trim()); cmd.Parameters.AddWithValue("@ND", nguoiDuyetId); cmd.ExecuteNonQuery(); }
            }
            return (true, "Hoàn tất nhập thiết bị.");
        }

        #endregion

        #region ── Lịch sử duyệt (BGH) ────────────────────────

        public List<object> GetLichSuDuyet(string idDX)
        {
            var list = new List<object>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"SELECT ls.CapDuyet, ls.ThoiGianDuyet, ls.TrangThaiSauDuyet, ls.GhiChu, nd.HoTen AS NguoiDuyet
                    FROM LICHSUDUYET ls JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=ls.NguoiDuyetNo
                    WHERE ls.DeXuatNo=@Id ORDER BY ls.ThoiGianDuyet";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idDX);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new
                            {
                                CapDuyet = r["CapDuyet"].ToString(),
                                NguoiDuyet = r["NguoiDuyet"].ToString(),
                                ThoiGian = Convert.ToDateTime(r["ThoiGianDuyet"]).ToString("dd/MM/yyyy HH:mm"),
                                TrangThaiSauDuyet = r["TrangThaiSauDuyet"].ToString(),
                                GhiChu = r["GhiChu"] == DBNull.Value ? "" : r["GhiChu"].ToString()
                            });
                }
            }
            return list;
        }

        public List<object> GetDanhSachDeXuat()
        {
            var list = new List<object>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"SELECT dx.ID_DeXuat, dx.NgayDeXuat, dx.TrangThai, nd.HoTen AS NguoiDeXuat, ISNULL(kp.TenPhongBanKhoa,'') AS KhoaPhongBan
                    FROM DEXUAT_MUASAM dx JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=dx.NguoiDeXuatNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=nd.Khoa_BanNo ORDER BY dx.NgayDeXuat DESC";
                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new
                        {
                            ID_DeXuat = r["ID_DeXuat"].ToString(),
                            NguoiDeXuat = r["NguoiDeXuat"].ToString(),
                            KhoaPhongBan = r["KhoaPhongBan"].ToString(),
                            NgayDeXuat = Convert.ToDateTime(r["NgayDeXuat"]).ToString("dd/MM/yyyy"),
                            TrangThai = r["TrangThai"].ToString()
                        });
            }
            return list;
        }

        #endregion
    }
}
