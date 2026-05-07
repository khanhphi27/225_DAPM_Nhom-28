using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace QLTB.Models.Repositories
{
    public class BaoTriRepository
    {
        #region ── Lập kế hoạch bảo trì ───────────────────────

        /// <summary>Dữ liệu trang LapKeHoachBaoTri.</summary>
        public LapKeHoachPageViewModel GetLapKeHoachData()
        {
            var vm = new LapKeHoachPageViewModel();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                // Báo hỏng chờ xử lý
                const string sqlBH = @"SELECT bh.ID_BaoHong, bh.ThietBiNo, tb.TenTB, ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                       ISNULL(kp.TenPhongBanKhoa,'') AS KhoaPhongBan, bh.NguoiBaoHongNo,
                       ISNULL(nd.HoTen,'') AS HoTenNguoiBao, bh.MoTaHong, bh.NgayBao, bh.MucDoUuTien, bh.TrangThai,
                       ISNULL((SELECT TOP 1 ckh.KeHoachNo FROM CHITIET_KEHOACH ckh WHERE ckh.BaoHongNo=bh.ID_BaoHong),'') AS KeHoachNo
                FROM BAOHONG_THIETBI bh JOIN THIETBI tb ON tb.ID_ThietBi=bh.ThietBiNo
                LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                LEFT JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=bh.NguoiBaoHongNo
                WHERE bh.TrangThai IN (N'Chờ xử lý',N'Đang xử lý') ORDER BY bh.NgayBao DESC";
                using (var cmd = new SqlCommand(sqlBH, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.BaoHongChoCho.Add(new BaoHongViewModel
                        {
                            ID_BaoHong = rd["ID_BaoHong"].ToString(), ThietBiNo = rd["ThietBiNo"].ToString(),
                            TenTB = rd["TenTB"].ToString(), TenDanhMuc = rd["TenDanhMuc"].ToString(),
                            KhoaPhongBan = rd["KhoaPhongBan"].ToString(), NguoiBaoHongNo = rd["NguoiBaoHongNo"].ToString(),
                            HoTenNguoiBao = rd["HoTenNguoiBao"].ToString(), MoTaHong = rd["MoTaHong"].ToString(),
                            NgayBao = Convert.ToDateTime(rd["NgayBao"]), MucDoUuTien = rd["MucDoUuTien"].ToString(),
                            TrangThai = rd["TrangThai"].ToString(), KeHoachNo = rd["KeHoachNo"].ToString()
                        });

                // Danh sách kế hoạch
                const string sqlKH = @"SELECT kh.ID_KeHoach, kh.NguoiLapNo, ISNULL(nd.HoTen,'') AS HoTenNguoiLap,
                       kh.NgayLap, kh.NgayDuKienHT, kh.LoaiKeHoach, kh.DonViThucHien, kh.ChiPhiDuKien, kh.TrangThai, kh.GhiChu
                FROM KEHOACH_BAOTRI kh LEFT JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=kh.NguoiLapNo ORDER BY kh.NgayLap DESC";
                using (var cmd = new SqlCommand(sqlKH, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.DanhSachKeHoach.Add(new KeHoachViewModel
                        {
                            ID_KeHoach = rd["ID_KeHoach"].ToString(), NguoiLapNo = rd["NguoiLapNo"].ToString(),
                            HoTenNguoiLap = rd["HoTenNguoiLap"].ToString(), NgayLap = Convert.ToDateTime(rd["NgayLap"]),
                            NgayDuKienHT = rd["NgayDuKienHT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayDuKienHT"]),
                            LoaiKeHoach = rd["LoaiKeHoach"].ToString(), DonViThucHien = rd["DonViThucHien"]?.ToString() ?? "",
                            ChiPhiDuKien = rd["ChiPhiDuKien"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["ChiPhiDuKien"]),
                            TrangThai = rd["TrangThai"].ToString(), GhiChu = rd["GhiChu"]?.ToString() ?? ""
                        });

                // Thiết bị dropdown
                const string sqlTB = @"SELECT tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB, ISNULL(dm.TenDanhMuc,'') AS DanhMuc,
                       CASE WHEN EXISTS (SELECT 1 FROM BAOHONG_THIETBI bh WHERE bh.ThietBiNo=tb.ID_ThietBi AND bh.TrangThai IN (N'Chờ xử lý',N'Đang xử lý')) THEN 1 ELSE 0 END AS DangBaoHong,
                       ISNULL((SELECT TOP 1 bh.ID_BaoHong FROM BAOHONG_THIETBI bh WHERE bh.ThietBiNo=tb.ID_ThietBi AND bh.TrangThai IN (N'Chờ xử lý',N'Đang xử lý')),'') AS BaoHongNo,
                       ISNULL((SELECT TOP 1 bh.MoTaHong FROM BAOHONG_THIETBI bh WHERE bh.ThietBiNo=tb.ID_ThietBi AND bh.TrangThai IN (N'Chờ xử lý',N'Đang xử lý')),'') AS MoTaBaoHong,
                       ISNULL((SELECT TOP 1 bh.MucDoUuTien FROM BAOHONG_THIETBI bh WHERE bh.ThietBiNo=tb.ID_ThietBi AND bh.TrangThai IN (N'Chờ xử lý',N'Đang xử lý')),'') AS MucDoUuTienBaoHong
                FROM THIETBI tb LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo ORDER BY tb.TenTB";
                using (var cmd = new SqlCommand(sqlTB, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.DanhSachThietBi.Add(new ThietBiDropdownViewModel
                        {
                            ID_ThietBi = rd["ID_ThietBi"].ToString(), TenTB = rd["TenTB"].ToString(),
                            TrangThai = rd["TrangThaiTB"].ToString(), DanhMuc = rd["DanhMuc"].ToString(),
                            DangBaoHong = Convert.ToInt32(rd["DangBaoHong"]) == 1,
                            BaoHongNo = rd["BaoHongNo"].ToString(), MoTaBaoHong = rd["MoTaBaoHong"].ToString(),
                            MucDoUuTienBaoHong = rd["MucDoUuTienBaoHong"].ToString()
                        });
            }
            return vm;
        }

        /// <summary>Lưu kế hoạch bảo trì + chi tiết.</summary>
        public (bool ok, string msg) LuuKeHoach(string idKH, string loai, string donVi, string ngayDuKienHT,
            string chiPhi, string ghiChu, string[] thietBiNos, string[] baoHongNos, string[] nguonGocs, string[] ghiChuCTs, string nguoiLapId)
        {
            if (string.IsNullOrWhiteSpace(idKH)) return (false, "Thiếu mã kế hoạch.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        decimal? cp = null;
                        if (!string.IsNullOrWhiteSpace(chiPhi)) cp = decimal.Parse(chiPhi.Replace(",", "").Replace(".", ""));

                        using (var cmd = new SqlCommand(@"INSERT INTO KEHOACH_BAOTRI (ID_KeHoach,NguoiLapNo,NgayLap,NgayDuKienHT,LoaiKeHoach,DonViThucHien,ChiPhiDuKien,TrangThai,GhiChu)
                            VALUES (@ID,@NL,GETDATE(),@NGHT,@Loai,@DV,@CP,N'Chờ thực hiện',@GC)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@ID", idKH);
                            cmd.Parameters.AddWithValue("@NL", nguoiLapId);
                            cmd.Parameters.AddWithValue("@NGHT", string.IsNullOrWhiteSpace(ngayDuKienHT) ? (object)DBNull.Value : DateTime.Parse(ngayDuKienHT));
                            cmd.Parameters.AddWithValue("@Loai", loai ?? "Sửa chữa");
                            cmd.Parameters.AddWithValue("@DV", (object)donVi ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CP", cp.HasValue ? (object)cp.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@GC", (object)ghiChu ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        if (thietBiNos != null)
                            for (int i = 0; i < thietBiNos.Length; i++)
                            {
                                if (string.IsNullOrWhiteSpace(thietBiNos[i])) continue;
                                string ctId;
                                using (var cmd2 = new SqlCommand("SELECT LEFT(REPLACE(NEWID(),'-',''),10)", conn, tran))
                                    ctId = cmd2.ExecuteScalar().ToString();

                                using (var cmd = new SqlCommand(@"INSERT INTO CHITIET_KEHOACH (ID_ChiTietKH,KeHoachNo,ThietBiNo,BaoHongNo,NguonGoc,GhiChuChiTiet)
                                    VALUES (@ID,@KH,@TB,@BH,@NG,@GC)", conn, tran))
                                {
                                    cmd.Parameters.AddWithValue("@ID", ctId);
                                    cmd.Parameters.AddWithValue("@KH", idKH);
                                    cmd.Parameters.AddWithValue("@TB", thietBiNos[i]);
                                    cmd.Parameters.AddWithValue("@BH", baoHongNos != null && i < baoHongNos.Length && !string.IsNullOrWhiteSpace(baoHongNos[i]) ? (object)baoHongNos[i] : DBNull.Value);
                                    cmd.Parameters.AddWithValue("@NG", nguonGocs != null && i < nguonGocs.Length ? (object)nguonGocs[i] : "Định kỳ");
                                    cmd.Parameters.AddWithValue("@GC", ghiChuCTs != null && i < ghiChuCTs.Length ? (object)ghiChuCTs[i] : DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                }
                                // Cập nhật trạng thái báo hỏng
                                if (baoHongNos != null && i < baoHongNos.Length && !string.IsNullOrWhiteSpace(baoHongNos[i]))
                                    using (var cmd = new SqlCommand("UPDATE BAOHONG_THIETBI SET TrangThai=N'Đang xử lý' WHERE ID_BaoHong=@BH AND TrangThai=N'Chờ xử lý'", conn, tran))
                                    { cmd.Parameters.AddWithValue("@BH", baoHongNos[i]); cmd.ExecuteNonQuery(); }
                            }
                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
            return (true, "Đã lưu kế hoạch " + idKH);
        }

        /// <summary>Xóa kế hoạch bảo trì.</summary>
        public (bool ok, string msg) XoaKeHoach(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Thiếu mã.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand("DELETE FROM CHITIET_KEHOACH WHERE KeHoachNo=@id", conn, tran))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    using (var cmd = new SqlCommand("DELETE FROM KEHOACH_BAOTRI WHERE ID_KeHoach=@id", conn, tran))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    tran.Commit();
                }
            }
            return (true, "Đã xóa kế hoạch.");
        }

        #endregion

        #region ── Ghi nhận sửa chữa ──────────────────────────

        public GhiNhanPageViewModel GetGhiNhanData()
        {
            var vm = new GhiNhanPageViewModel();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                // Kế hoạch đang/chờ thực hiện
                const string sqlKH = @"SELECT kh.ID_KeHoach, kh.LoaiKeHoach, kh.DonViThucHien, kh.ChiPhiDuKien, kh.TrangThai, kh.GhiChu,
                       kh.NgayLap, kh.NgayDuKienHT, ISNULL(nd.HoTen,'') AS HoTenNguoiLap
                FROM KEHOACH_BAOTRI kh LEFT JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=kh.NguoiLapNo
                WHERE kh.TrangThai IN (N'Chờ thực hiện',N'Đang thực hiện') ORDER BY kh.NgayLap DESC";
                var khList = new List<KeHoachViewModel>();
                using (var cmd = new SqlCommand(sqlKH, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        khList.Add(new KeHoachViewModel
                        {
                            ID_KeHoach = rd["ID_KeHoach"].ToString(), LoaiKeHoach = rd["LoaiKeHoach"].ToString(),
                            DonViThucHien = rd["DonViThucHien"]?.ToString() ?? "", ChiPhiDuKien = rd["ChiPhiDuKien"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["ChiPhiDuKien"]),
                            TrangThai = rd["TrangThai"].ToString(), GhiChu = rd["GhiChu"]?.ToString() ?? "",
                            NgayLap = Convert.ToDateTime(rd["NgayLap"]),
                            NgayDuKienHT = rd["NgayDuKienHT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayDuKienHT"]),
                            HoTenNguoiLap = rd["HoTenNguoiLap"].ToString()
                        });

                // Chi tiết kế hoạch
                var ctDict = new Dictionary<string, List<ChiTietKeHoachViewModel>>();
                const string sqlCT = @"SELECT ckh.ID_ChiTietKH, ckh.KeHoachNo, ckh.ThietBiNo, ISNULL(tb.TenTB,'—') AS TenTB,
                       ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc, ckh.BaoHongNo, ISNULL(bh.MoTaHong,'') AS MoTaHong,
                       ckh.NguonGoc, ckh.GhiChuChiTiet
                FROM CHITIET_KEHOACH ckh LEFT JOIN THIETBI tb ON tb.ID_ThietBi=ckh.ThietBiNo
                LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo LEFT JOIN BAOHONG_THIETBI bh ON bh.ID_BaoHong=ckh.BaoHongNo
                WHERE ckh.KeHoachNo IN (SELECT kh.ID_KeHoach FROM KEHOACH_BAOTRI kh WHERE kh.TrangThai IN (N'Chờ thực hiện',N'Đang thực hiện'))
                  AND NOT EXISTS (SELECT 1 FROM GHINHAN_SUA_CHUA gn WHERE gn.ChiTietKeHoachNo=ckh.ID_ChiTietKH)";
                using (var cmd = new SqlCommand(sqlCT, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                    {
                        var kh = rd["KeHoachNo"].ToString();
                        if (!ctDict.ContainsKey(kh)) ctDict[kh] = new List<ChiTietKeHoachViewModel>();
                        ctDict[kh].Add(new ChiTietKeHoachViewModel
                        {
                            ID_ChiTietKH = rd["ID_ChiTietKH"].ToString(), KeHoachNo = kh,
                            ThietBiNo = rd["ThietBiNo"] == DBNull.Value ? null : rd["ThietBiNo"].ToString(),
                            TenTB = rd["TenTB"].ToString(), TenDanhMuc = rd["TenDanhMuc"].ToString(),
                            BaoHongNo = rd["BaoHongNo"] == DBNull.Value ? null : rd["BaoHongNo"].ToString(),
                            MoTaBaoHong = rd["MoTaHong"].ToString(), NguonGoc = rd["NguonGoc"].ToString(),
                            GhiChuChiTiet = rd["GhiChuChiTiet"].ToString()
                        });
                    }
                foreach (var kh in khList)
                    if (ctDict.ContainsKey(kh.ID_KeHoach)) kh.ChiTiet = ctDict[kh.ID_KeHoach];
                foreach (var kh in khList)
                    if (kh.ChiTiet != null && kh.ChiTiet.Count > 0) vm.KeHoachChoCho.Add(kh);

                // Lịch sử ghi nhận
                const string sqlGN = @"SELECT TOP 20 gn.ID_GhiNhan, gn.KeHoachNo, ISNULL(kh.LoaiKeHoach,'') AS LoaiKeHoach,
                       gn.ChiTietKeHoachNo, ckh.ThietBiNo, ISNULL(tb.TenTB,'—') AS TenTB,
                       ckh.BaoHongNo, ISNULL(bh.MoTaHong,'') AS MoTaHong, ISNULL(kh.DonViThucHien,'') AS DonViThucHien,
                       gn.NgayThucHien, ISNULL(gn.KetQua,'') AS KetQua, gn.ChiPhiThucTe, ISNULL(gn.TrangThaiSauSua,'') AS TrangThaiSauSua
                FROM GHINHAN_SUA_CHUA gn JOIN KEHOACH_BAOTRI kh ON kh.ID_KeHoach=gn.KeHoachNo
                LEFT JOIN CHITIET_KEHOACH ckh ON ckh.ID_ChiTietKH=gn.ChiTietKeHoachNo
                LEFT JOIN THIETBI tb ON tb.ID_ThietBi=ckh.ThietBiNo
                LEFT JOIN BAOHONG_THIETBI bh ON bh.ID_BaoHong=ckh.BaoHongNo ORDER BY gn.NgayThucHien DESC";
                using (var cmd = new SqlCommand(sqlGN, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.LichSuGhiNhan.Add(new GhiNhanViewModel
                        {
                            ID_GhiNhan = rd["ID_GhiNhan"].ToString(), KeHoachNo = rd["KeHoachNo"].ToString(),
                            LoaiKeHoach = rd["LoaiKeHoach"].ToString(),
                            ChiTietKeHoachNo = rd["ChiTietKeHoachNo"] == DBNull.Value ? null : rd["ChiTietKeHoachNo"].ToString(),
                            ThietBiNo = rd["ThietBiNo"] == DBNull.Value ? null : rd["ThietBiNo"].ToString(),
                            TenTB = rd["TenTB"].ToString(),
                            BaoHongNo = rd["BaoHongNo"] == DBNull.Value ? null : rd["BaoHongNo"].ToString(),
                            MoTaBaoHong = rd["MoTaHong"].ToString(), DonViThucHien = rd["DonViThucHien"].ToString(),
                            NgayThucHien = Convert.ToDateTime(rd["NgayThucHien"]), KetQua = rd["KetQua"].ToString(),
                            ChiPhiThucTe = rd["ChiPhiThucTe"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["ChiPhiThucTe"]),
                            TrangThaiSauSua = rd["TrangThaiSauSua"].ToString()
                        });
            }
            return vm;
        }

        /// <summary>Lưu ghi nhận sửa chữa + cập nhật trạng thái.</summary>
        public (bool ok, string msg, string trangThaiKH) LuuGhiNhan(string idGhiNhan, string keHoachNo,
            string chiTietKeHoachNo, string ngayThucHien, string ketQua, string chiPhiThucTe, string trangThaiSauSua)
        {
            if (string.IsNullOrWhiteSpace(idGhiNhan) || string.IsNullOrWhiteSpace(keHoachNo))
                return (false, "Thiếu thông tin bắt buộc.", null);

            decimal? chiPhi = null;
            if (!string.IsNullOrWhiteSpace(chiPhiThucTe))
                chiPhi = decimal.Parse(chiPhiThucTe.Replace(",", "").Replace(".", ""));

            string ttKH = "";
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(@"INSERT INTO GHINHAN_SUA_CHUA (ID_GhiNhan,KeHoachNo,ChiTietKeHoachNo,NgayThucHien,KetQua,ChiPhiThucTe,TrangThaiSauSua)
                        VALUES (@ID,@KH,@CTKH,@Ngay,@KQ,@CP,@TT)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@ID", idGhiNhan);
                        cmd.Parameters.AddWithValue("@KH", keHoachNo);
                        cmd.Parameters.AddWithValue("@CTKH", string.IsNullOrWhiteSpace(chiTietKeHoachNo) ? (object)DBNull.Value : chiTietKeHoachNo);
                        cmd.Parameters.AddWithValue("@Ngay", DateTime.Parse(ngayThucHien));
                        cmd.Parameters.AddWithValue("@KQ", (object)ketQua ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CP", chiPhi.HasValue ? (object)chiPhi.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TT", (object)trangThaiSauSua ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    if (!string.IsNullOrWhiteSpace(chiTietKeHoachNo) && !string.IsNullOrWhiteSpace(trangThaiSauSua))
                    {
                        string tbStatus = trangThaiSauSua.Contains("Hoạt động tốt") ? "Đang sử dụng"
                                         : trangThaiSauSua.Contains("Tạm thời") ? "Cần bảo trì" : "Báo hỏng";
                        using (var cmd = new SqlCommand("UPDATE tb SET tb.TrangThaiTB=@S FROM THIETBI tb JOIN CHITIET_KEHOACH ckh ON ckh.ThietBiNo=tb.ID_ThietBi WHERE ckh.ID_ChiTietKH=@CTKH", conn, tran))
                        { cmd.Parameters.AddWithValue("@S", tbStatus); cmd.Parameters.AddWithValue("@CTKH", chiTietKeHoachNo); cmd.ExecuteNonQuery(); }
                        using (var cmd = new SqlCommand("UPDATE bh SET bh.TrangThai=N'Đã xử lý' FROM BAOHONG_THIETBI bh JOIN CHITIET_KEHOACH ckh ON ckh.BaoHongNo=bh.ID_BaoHong WHERE ckh.ID_ChiTietKH=@CTKH AND bh.TrangThai=N'Đang xử lý'", conn, tran))
                        { cmd.Parameters.AddWithValue("@CTKH", chiTietKeHoachNo); cmd.ExecuteNonQuery(); }
                    }

                    int tongCT = 0, daGN = 0;
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM CHITIET_KEHOACH WHERE KeHoachNo=@KH", conn, tran))
                    { cmd.Parameters.AddWithValue("@KH", keHoachNo); tongCT = Convert.ToInt32(cmd.ExecuteScalar()); }
                    using (var cmd = new SqlCommand("SELECT COUNT(DISTINCT ChiTietKeHoachNo) FROM GHINHAN_SUA_CHUA WHERE KeHoachNo=@KH AND ChiTietKeHoachNo IS NOT NULL", conn, tran))
                    { cmd.Parameters.AddWithValue("@KH", keHoachNo); daGN = Convert.ToInt32(cmd.ExecuteScalar()); }
                    ttKH = (daGN >= tongCT && tongCT > 0) ? "Đã hoàn thành" : "Đang thực hiện";
                    using (var cmd = new SqlCommand("UPDATE KEHOACH_BAOTRI SET TrangThai=@TT WHERE ID_KeHoach=@KH", conn, tran))
                    { cmd.Parameters.AddWithValue("@TT", ttKH); cmd.Parameters.AddWithValue("@KH", keHoachNo); cmd.ExecuteNonQuery(); }

                    tran.Commit();
                }
            }
            return (true, "Đã lưu ghi nhận " + idGhiNhan, ttKH);
        }

        #endregion
    }
}
