using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace QLTB.Models.Repositories
{
    public class ThongKeRepository
    {
        #region ── BGH Dashboard ───────────────────────────────

        public BGHDashboardViewModel GetBGHDashboard()
        {
            var vm = new BGHDashboardViewModel { HoatDongGanDay = new List<HoatDongGanDayViewModel>() };
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"SELECT COUNT(*) AS Tong,
                    SUM(CASE WHEN TrangThaiTB=N'Đang sử dụng' THEN 1 ELSE 0 END) AS HD,
                    SUM(CASE WHEN TrangThaiTB=N'Cần bảo trì' THEN 1 ELSE 0 END) AS BT,
                    SUM(CASE WHEN TrangThaiTB=N'Báo hỏng' THEN 1 ELSE 0 END) AS H FROM THIETBI", conn))
                using (var rd = cmd.ExecuteReader())
                    if (rd.Read()) { vm.TongThietBi = Convert.ToInt32(rd["Tong"]); vm.HoatDong = Convert.ToInt32(rd["HD"]); vm.BaoTri = Convert.ToInt32(rd["BT"]); vm.Hong = Convert.ToInt32(rd["H"]); }

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM DEXUAT_MUASAM WHERE TrangThai IN (N'Chờ CSVC duyệt',N'Chờ KHTC duyệt',N'Chờ BGH duyệt')", conn))
                    vm.ChoDeXuatDuyet = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new SqlCommand(@"SELECT TOP 10 dx.ID_DeXuat, dx.MoTa, nd.HoTen AS NguoiDeXuat,
                    ISNULL(kp.TenPhongBanKhoa,'') AS KhoaPhongBan, dx.NgayDeXuat, dx.TrangThai
                    FROM DEXUAT_MUASAM dx JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=dx.NguoiDeXuatNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=nd.Khoa_BanNo ORDER BY dx.NgayDeXuat DESC", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.HoatDongGanDay.Add(new HoatDongGanDayViewModel
                        {
                            ID_DeXuat = rd["ID_DeXuat"].ToString(), MoTa = rd["MoTa"]?.ToString() ?? "",
                            NguoiDeXuat = rd["NguoiDeXuat"].ToString(), KhoaPhongBan = rd["KhoaPhongBan"].ToString(),
                            NgayDeXuat = Convert.ToDateTime(rd["NgayDeXuat"]), TrangThai = rd["TrangThai"].ToString()
                        });
            }
            return vm;
        }

        #endregion

        #region ── BGH Thống kê tài sản ────────────────────────

        public ThongKeTaiSanViewModel GetThongKeTaiSan()
        {
            var vm = new ThongKeTaiSanViewModel
            {
                TheoKhoa = new List<ThongKeTheoKhoaViewModel>(),
                TheoDanhMuc = new List<ThongKeTheoDanhMucViewModel>(),
                LichSuKiemKe = new List<KiemKeListViewModel>(),
                CanChuY = new List<ThietBiCanChuYViewModel>()
            };
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                // Tổng hợp
                using (var cmd = new SqlCommand(@"SELECT COUNT(*) AS Tong,
                    SUM(CASE WHEN TrangThaiTB=N'Đang sử dụng' THEN 1 ELSE 0 END) AS HD,
                    SUM(CASE WHEN TrangThaiTB=N'Cần bảo trì' THEN 1 ELSE 0 END) AS BT,
                    SUM(CASE WHEN TrangThaiTB=N'Báo hỏng' THEN 1 ELSE 0 END) AS H,
                    ISNULL(SUM(Gia),0) AS TG FROM THIETBI", conn))
                using (var rd = cmd.ExecuteReader())
                    if (rd.Read()) { vm.TongThietBi = Convert.ToInt32(rd["Tong"]); vm.HoatDong = Convert.ToInt32(rd["HD"]); vm.BaoTri = Convert.ToInt32(rd["BT"]); vm.Hong = Convert.ToInt32(rd["H"]); vm.TongGiaTri = Convert.ToDecimal(rd["TG"]); }

                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(ChiPhiThucTe),0) FROM GHINHAN_SUA_CHUA", conn))
                    vm.ChiPhiBaoTri = Convert.ToDecimal(cmd.ExecuteScalar());
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM BAOHONG_THIETBI", conn))
                    vm.TongBaoHong = Convert.ToInt32(cmd.ExecuteScalar());

                // Theo khoa
                const string sqlKhoa = @"SELECT ISNULL(kp.TenPhongBanKhoa,N'Chưa phân') AS TenKhoa, COUNT(*) AS Tong,
                    SUM(CASE WHEN tb.TrangThaiTB=N'Đang sử dụng' THEN 1 ELSE 0 END) AS HD,
                    SUM(CASE WHEN tb.TrangThaiTB=N'Cần bảo trì' THEN 1 ELSE 0 END) AS BT,
                    SUM(CASE WHEN tb.TrangThaiTB=N'Báo hỏng' THEN 1 ELSE 0 END) AS H,
                    ISNULL(SUM(tb.Gia),0) AS TG
                    FROM THIETBI tb LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan GROUP BY kp.TenPhongBanKhoa";
                using (var cmd = new SqlCommand(sqlKhoa, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.TheoKhoa.Add(new ThongKeTheoKhoaViewModel { TenKhoa = rd["TenKhoa"].ToString(), Tong = Convert.ToInt32(rd["Tong"]), HoatDong = Convert.ToInt32(rd["HD"]), BaoTri = Convert.ToInt32(rd["BT"]), Hong = Convert.ToInt32(rd["H"]), TongGia = Convert.ToDecimal(rd["TG"]) });

                // Theo danh mục
                using (var cmd = new SqlCommand(@"SELECT ISNULL(dm.TenDanhMuc,N'Khác') AS TenDanhMuc, COUNT(*) AS SL
                    FROM THIETBI tb LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo GROUP BY dm.TenDanhMuc", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.TheoDanhMuc.Add(new ThongKeTheoDanhMucViewModel { TenDanhMuc = rd["TenDanhMuc"].ToString(), SoLuong = Convert.ToInt32(rd["SL"]) });

                // Kiểm kê
                using (var cmd = new SqlCommand(@"SELECT kk.ID_KiemKe, kk.NgayKiemKe, ISNULL(nd.HoTen,'') AS NguoiThucHien,
                    kk.TrangThai, kk.GhiChu, (SELECT COUNT(*) FROM CHITIET_KIEMKE ckk WHERE ckk.KiemKeNo=kk.ID_KiemKe) AS TongTB
                    FROM KIEMKE kk LEFT JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=kk.NguoiThucHienNo ORDER BY kk.NgayKiemKe DESC", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.LichSuKiemKe.Add(new KiemKeListViewModel { ID_KiemKe = rd["ID_KiemKe"].ToString(), NgayKiemKe = Convert.ToDateTime(rd["NgayKiemKe"]), NguoiThucHien = rd["NguoiThucHien"].ToString(), TrangThai = rd["TrangThai"]?.ToString() ?? "", GhiChu = rd["GhiChu"]?.ToString() ?? "", TongThietBiKK = Convert.ToInt32(rd["TongTB"]) });

                // Thiết bị cần chú ý
                using (var cmd = new SqlCommand(@"SELECT TOP 10 tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB,
                    ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc, ISNULL(kp.TenPhongBanKhoa,'') AS KhoaPhongBan,
                    tb.Gia, bh.MoTaHong, bh.NgayBao
                    FROM THIETBI tb LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                    LEFT JOIN BAOHONG_THIETBI bh ON bh.ThietBiNo=tb.ID_ThietBi AND bh.TrangThai IN (N'Chờ xử lý',N'Đang xử lý')
                    WHERE tb.TrangThaiTB IN (N'Báo hỏng',N'Cần bảo trì') ORDER BY bh.NgayBao DESC", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.CanChuY.Add(new ThietBiCanChuYViewModel { ID_ThietBi = rd["ID_ThietBi"].ToString(), TenTB = rd["TenTB"].ToString(), TrangThaiTB = rd["TrangThaiTB"].ToString(), TenDanhMuc = rd["TenDanhMuc"].ToString(), KhoaPhongBan = rd["KhoaPhongBan"].ToString(), Gia = rd["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["Gia"]), MoTaHong = rd["MoTaHong"]?.ToString(), NgayBaoHong = rd["NgayBao"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayBao"]) });
            }
            return vm;
        }

        #endregion

        #region ── BGH Theo dõi ────────────────────────────────

        public List<TheDoiThietBiViewModel> GetTheoDoi()
        {
            var list = new List<TheDoiThietBiViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"SELECT tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB,
                    ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc, ISNULL(kp.TenPhongBanKhoa,'') AS KhoaPhongBan,
                    tb.Gia, ISNULL(ncc.TenNhaCC,'') AS NhaCungCap,
                    ISNULL(p.TenPhong,'') AS Phong, bh.MoTaHong, bh.NgayBao, ISNULL(ndBH.HoTen,'') AS NguoiBaoHong
                    FROM THIETBI tb LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                    LEFT JOIN NHACUNGCAP ncc ON ncc.ID_NhaCC=tb.NhaCCNo
                    OUTER APPLY (SELECT TOP 1 ptb.PhongNo FROM PHONG_THIETBI ptb WHERE ptb.ThietBiNo=tb.ID_ThietBi ORDER BY ptb.NgayHieuLuc DESC) ptb
                    LEFT JOIN PHONG p ON p.ID_Phong=ptb.PhongNo
                    OUTER APPLY (SELECT TOP 1 bh.MoTaHong, bh.NgayBao, bh.NguoiBaoHongNo FROM BAOHONG_THIETBI bh WHERE bh.ThietBiNo=tb.ID_ThietBi ORDER BY bh.NgayBao DESC) bh
                    LEFT JOIN NGUOIDUNG ndBH ON ndBH.ID_NguoiDung=bh.NguoiBaoHongNo
                    ORDER BY tb.TenTB";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new TheDoiThietBiViewModel
                        {
                            ID_ThietBi = rd["ID_ThietBi"].ToString(), TenTB = rd["TenTB"].ToString(),
                            TrangThaiTB = rd["TrangThaiTB"]?.ToString() ?? "", TenDanhMuc = rd["TenDanhMuc"].ToString(),
                            KhoaPhongBan = rd["KhoaPhongBan"].ToString(), Gia = rd["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["Gia"]),
                            NhaCungCap = rd["NhaCungCap"].ToString(), Phong = rd["Phong"].ToString(),
                            MoTaHong = rd["MoTaHong"]?.ToString(), NgayBaoHong = rd["NgayBao"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayBao"]),
                            NguoiBaoHong = rd["NguoiBaoHong"].ToString()
                        });
            }
            return list;
        }

        public List<object> GetLichSuDuyetTheoThietBi(string maTB, string tenTB)
        {
            var list = new List<object>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();

                // 1. Tìm theo ID_ThietBi -> DeXuatNo
                if (!string.IsNullOrEmpty(maTB))
                {
                    string sqlByMaTB = @"
                        SELECT ls.CapDuyet, ls.ThoiGianDuyet, ls.TrangThaiSauDuyet, ls.GhiChu,
                               nd.HoTen AS NguoiDuyet,
                               dx.ID_DeXuat, dx.TrangThai AS TrangThaiDeXuat,
                               dx.NgayDeXuat, dx.MoTa, tb.TenTB AS TenThietBiDeXuat
                        FROM THIETBI tb
                        JOIN DEXUAT_MUASAM dx ON dx.ID_DeXuat = tb.DeXuatNo
                        JOIN LICHSUDUYET ls ON ls.DeXuatNo = dx.ID_DeXuat
                        JOIN NGUOIDUNG nd ON nd.ID_NguoiDung = ls.NguoiDuyetNo
                        WHERE tb.ID_ThietBi = @maTB
                        ORDER BY dx.NgayDeXuat DESC, ls.ThoiGianDuyet";

                    using (var cmd = new SqlCommand(sqlByMaTB, conn))
                    {
                        cmd.Parameters.AddWithValue("@maTB", maTB);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(new
                                {
                                    CapDuyet = r["CapDuyet"].ToString(),
                                    NguoiDuyet = r["NguoiDuyet"].ToString(),
                                    ThoiGian = Convert.ToDateTime(r["ThoiGianDuyet"]).ToString("dd/MM/yyyy HH:mm"),
                                    TrangThaiSauDuyet = r["TrangThaiSauDuyet"].ToString(),
                                    GhiChu = r["GhiChu"] == DBNull.Value ? "" : r["GhiChu"].ToString(),
                                    ID_DeXuat = r["ID_DeXuat"].ToString(),
                                    TrangThaiDeXuat = r["TrangThaiDeXuat"].ToString(),
                                    NgayDeXuat = Convert.ToDateTime(r["NgayDeXuat"]).ToString("dd/MM/yyyy"),
                                    MoTa = r["MoTa"] == DBNull.Value ? "" : r["MoTa"].ToString(),
                                    TenThietBiDeXuat = r["TenThietBiDeXuat"].ToString()
                                });
                            }
                        }
                    }
                }

                // 2. Nếu không tìm thấy (có thể thiết bị cũ không có DeXuatNo), tìm theo tên
                if (list.Count == 0 && !string.IsNullOrEmpty(tenTB))
                {
                    var words = tenTB.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        var likes = new List<string>();
                        for (int i = 0; i < words.Length; i++) likes.Add("ct.TenThietBiDeXuat LIKE @W" + i);

                        string sqlFallback = @"
                            SELECT ls.CapDuyet, ls.ThoiGianDuyet, ls.TrangThaiSauDuyet, ls.GhiChu,
                                   nd.HoTen AS NguoiDuyet,
                                   dx.ID_DeXuat, dx.TrangThai AS TrangThaiDeXuat,
                                   dx.NgayDeXuat, dx.MoTa, ct.TenThietBiDeXuat
                            FROM LICHSUDUYET ls
                            JOIN NGUOIDUNG nd ON nd.ID_NguoiDung = ls.NguoiDuyetNo
                            JOIN DEXUAT_MUASAM dx ON dx.ID_DeXuat = ls.DeXuatNo
                            JOIN CHITIET_DEXUAT ct ON ct.DeXuatNo = dx.ID_DeXuat
                            WHERE (" + string.Join(" OR ", likes) + @")
                            ORDER BY dx.NgayDeXuat DESC, ls.ThoiGianDuyet";

                        using (var cmd = new SqlCommand(sqlFallback, conn))
                        {
                            for (int i = 0; i < words.Length; i++)
                                cmd.Parameters.AddWithValue("@W" + i, "%" + words[i] + "%");
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    list.Add(new
                                    {
                                        CapDuyet = r["CapDuyet"].ToString(),
                                        NguoiDuyet = r["NguoiDuyet"].ToString(),
                                        ThoiGian = Convert.ToDateTime(r["ThoiGianDuyet"]).ToString("dd/MM/yyyy HH:mm"),
                                        TrangThaiSauDuyet = r["TrangThaiSauDuyet"].ToString(),
                                        GhiChu = r["GhiChu"] == DBNull.Value ? "" : r["GhiChu"].ToString(),
                                        ID_DeXuat = r["ID_DeXuat"].ToString(),
                                        TrangThaiDeXuat = r["TrangThaiDeXuat"].ToString(),
                                        NgayDeXuat = Convert.ToDateTime(r["NgayDeXuat"]).ToString("dd/MM/yyyy"),
                                        MoTa = r["MoTa"] == DBNull.Value ? "" : r["MoTa"].ToString(),
                                        TenThietBiDeXuat = r["TenThietBiDeXuat"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        #endregion

        #region ── KHTC Dashboard ──────────────────────────────

        public KHTCDashboardViewModel GetKHTCDashboard()
        {
            var vm = new KHTCDashboardViewModel();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Gia),0) FROM THIETBI", conn))
                    vm.TongGiaTri = Convert.ToDecimal(cmd.ExecuteScalar());
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(ChiPhiThucTe),0) FROM GHINHAN_SUA_CHUA", conn))
                    vm.TongSuaChua = Convert.ToDecimal(cmd.ExecuteScalar());
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM DEXUAT_MUASAM WHERE TrangThai=N'Chờ KHTC duyệt'", conn))
                    vm.ChoDuyet = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new SqlCommand(@"SELECT TOP 10 dx.ID_DeXuat, nd.HoTen AS NguoiDeXuat,
                    ISNULL(kp.TenPhongBanKhoa,'') AS KhoaPhongBan, dx.NgayDeXuat, dx.TrangThai, dx.MoTa,
                    ISNULL((SELECT SUM(ct.SoLuong*ISNULL(ct.GiaDuKien,0)) FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo=dx.ID_DeXuat),0) AS TongGia
                    FROM DEXUAT_MUASAM dx JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=dx.NguoiDeXuatNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=nd.Khoa_BanNo ORDER BY dx.NgayDeXuat DESC", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.HoatDongGanDay.Add(new DeXuatViewModel
                        {
                            ID_DeXuat = rd["ID_DeXuat"].ToString(), NguoiDeXuat = rd["NguoiDeXuat"].ToString(),
                            KhoaPhongBan = rd["KhoaPhongBan"].ToString(), NgayDeXuat = Convert.ToDateTime(rd["NgayDeXuat"]),
                            TrangThai = rd["TrangThai"].ToString(), MoTa = rd["MoTa"]?.ToString() ?? "",
                            TongGiaDuKien = Convert.ToDecimal(rd["TongGia"])
                        });
            }
            return vm;
        }

        #endregion

        #region ── KHTC Báo cáo tài sản ────────────────────────

        public List<BaoCaoTaiSanViewModel> GetBaoCaoTaiSan()
        {
            var list = new List<BaoCaoTaiSanViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, ISNULL(CONVERT(VARCHAR,tb.SoSeri),'') AS SoSeri,
                           ISNULL(tb.ThongSoKT,'') AS ThongSoKT,
                           ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                           ISNULL(kp.TenPhongBanKhoa,'') AS TenPhongBanKhoa,
                           ISNULL(tb.Gia,0) AS Gia, ISNULL(tb.TrangThaiTB,'') AS TrangThaiTB,
                           ISNULL((SELECT SUM(gn.ChiPhiThucTe) FROM GHINHAN_SUA_CHUA gn
                                   JOIN CHITIET_KEHOACH ckh ON ckh.ID_ChiTietKH=gn.ChiTietKeHoachNo
                                   WHERE ckh.ThietBiNo=tb.ID_ThietBi),0) AS TongChiPhiSuaChua
                    FROM THIETBI tb
                    LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                    ORDER BY tb.TenTB";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new BaoCaoTaiSanViewModel
                        {
                            ID_ThietBi = rd["ID_ThietBi"].ToString(), TenTB = rd["TenTB"].ToString(),
                            SoSeri = rd["SoSeri"].ToString(), ThongSoKT = rd["ThongSoKT"].ToString(),
                            TenDanhMuc = rd["TenDanhMuc"].ToString(), TenPhongBanKhoa = rd["TenPhongBanKhoa"].ToString(),
                            Gia = Convert.ToDecimal(rd["Gia"]), TrangThaiTB = rd["TrangThaiTB"].ToString(),
                            TongChiPhiSuaChua = Convert.ToDecimal(rd["TongChiPhiSuaChua"])
                        });
            }
            return list;
        }

        #endregion
    }
}
