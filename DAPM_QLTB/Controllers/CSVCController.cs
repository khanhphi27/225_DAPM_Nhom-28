using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using QLTB.Models;

namespace QLTB.Controllers
{
    public class CSVCController : Controller
    {
        private ActionResult CheckAuth()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return null;
        }
        private string CurrentUser => Session["UserId"]?.ToString() ?? "csvc";

        public ActionResult QuanLyThietBi()
        {
            var r = CheckAuth(); if (r != null) return r;

            var list = new List<ThietBiViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB,
                           dm.TenDanhMuc,
                           kp.TenPhongBanKhoa,
                           ncc.TenNhaCC,
                           tb.SoSeri, tb.Gia, tb.TrangThaiTB
                    FROM THIETBI tb
                    LEFT JOIN DANHMUC dm ON tb.DanhMucNo = dm.ID_DanhMuc
                    LEFT JOIN KHOA_PHONGBAN kp ON tb.KhoaPhongBan = kp.ID_KhoaPhongBan
                    LEFT JOIN NHACUNGCAP ncc ON tb.NhaCCNo = ncc.ID_NhaCC";

                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new ThietBiViewModel
                        {
                            ID_ThietBi = rd["ID_ThietBi"].ToString(),
                            TenTB = rd["TenTB"].ToString(),
                            DanhMuc = rd["TenDanhMuc"]?.ToString() ?? "",
                            KhoaPhongBan = rd["TenPhongBanKhoa"]?.ToString() ?? "",
                            NhaCungCap = rd["TenNhaCC"]?.ToString() ?? "",
                            SoSeri = rd["SoSeri"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SoSeri"]),
                            Gia = rd["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["Gia"]),
                            TrangThaiTB = rd["TrangThaiTB"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return View(list);
        }
        // ══ KIỂM KÊ TÀI SẢN ════════════════════════════════════
        public ActionResult KiemKeTaiSan()
        {
            var r = CheckAuth(); if (r != null) return r;
            var vm = new KiemKeTaiSanViewModel();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    // Lấy tất cả thiết bị kèm kết quả kiểm kê gần nhất
                    const string sql = @"
                        SELECT tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB,
                               ISNULL(kp.TenPhongBanKhoa, N'Chưa phân khoa') AS TenKhoa,
                               ISNULL(dm.TenDanhMuc, '') AS TenDanhMuc,
                               ISNULL(ct.SoLuongHeThong, 1) AS SoLuongHeThong,
                               ct.SoLuongThucTe, ct.TinhTrangThucTe, ct.GhiChu,
                               kk.NgayKiemKe
                        FROM THIETBI tb
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = tb.KhoaPhongBan
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc = tb.DanhMucNo
                        LEFT JOIN (
                            SELECT ct2.ThietBiNo, ct2.SoLuongHeThong, ct2.SoLuongThucTe, ct2.TinhTrangThucTe, ct2.GhiChu, ct2.KiemKeNo,
                                   ROW_NUMBER() OVER (PARTITION BY ct2.ThietBiNo ORDER BY kk2.NgayKiemKe DESC) AS rn
                            FROM CHITIET_KIEMKE ct2
                            JOIN KIEMKE kk2 ON kk2.ID_KiemKe = ct2.KiemKeNo
                        ) ct ON ct.ThietBiNo = tb.ID_ThietBi AND ct.rn = 1
                        LEFT JOIN KIEMKE kk ON kk.ID_KiemKe = ct.KiemKeNo
                        ORDER BY TenKhoa, tb.TenTB";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            vm.DanhSachThietBi.Add(new ThietBiKiemKeRow
                            {
                                ID_ThietBi   = rd["ID_ThietBi"].ToString(),
                                TenTB        = rd["TenTB"].ToString(),
                                TrangThaiTB  = rd["TrangThaiTB"]?.ToString() ?? "",
                                TenKhoa      = rd["TenKhoa"].ToString(),
                                TenDanhMuc   = rd["TenDanhMuc"].ToString(),
                                SoLuongHeThong  = Convert.ToInt32(rd["SoLuongHeThong"]),
                                SoLuongThucTe   = rd["SoLuongThucTe"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SoLuongThucTe"]),
                                TinhTrangThucTe = rd["TinhTrangThucTe"]?.ToString(),
                                GhiChu          = rd["GhiChu"]?.ToString(),
                                NgayKiemKe      = rd["NgayKiemKe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayKiemKe"])
                            });

                    vm.TongThietBi   = vm.DanhSachThietBi.Count;
                    vm.DaKiemKe      = vm.DanhSachThietBi.Count(x => x.NgayKiemKe.HasValue);
                    vm.ChuaKiemKe    = vm.TongThietBi - vm.DaKiemKe;
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(vm);
        }

        public ActionResult TaoPhieuKiemKe()
        {
            var r = CheckAuth(); if (r != null) return r;
            var vm = new TaoPhieuKiemKeViewModel();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    // Chỉ lấy thiết bị CHƯA có trong bất kỳ phiếu kiểm kê nào
                    const string sql = @"
                        SELECT tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB,
                               ISNULL(kp.TenPhongBanKhoa, N'Chưa phân khoa') AS TenKhoa,
                               ISNULL(dm.TenDanhMuc, '') AS TenDanhMuc
                        FROM THIETBI tb
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = tb.KhoaPhongBan
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc = tb.DanhMucNo
                        WHERE NOT EXISTS (
                            SELECT 1 FROM CHITIET_KIEMKE ct WHERE ct.ThietBiNo = tb.ID_ThietBi
                        )
                        ORDER BY TenKhoa, tb.TenTB";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            vm.DanhSachChuaKiem.Add(new ItemTaoKiemKe
                            {
                                ID_ThietBi       = rd["ID_ThietBi"].ToString(),
                                TenTB            = rd["TenTB"].ToString(),
                                TrangThaiTB      = rd["TrangThaiTB"]?.ToString() ?? "",
                                TenKhoa          = rd["TenKhoa"].ToString(),
                                TenDanhMuc       = rd["TenDanhMuc"].ToString(),
                                SoLuongHeThong   = 1  // mỗi bản ghi THIETBI = 1 thiết bị vật lý
                            });

                    // Sinh ID phiếu
                    int soPhieu = 0;
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM KIEMKE", conn))
                        soPhieu = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    vm.ID_KiemKe  = "KK" + soPhieu.ToString("D8");
                    vm.NgayKiemKe = DateTime.Now;
                    vm.NguoiTao   = Session["HoTen"]?.ToString() ?? Session["UserId"]?.ToString() ?? "csvc";
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(vm);
        }

        [HttpPost]
        public ActionResult HoanTatKiemKe(TaoPhieuKiemKeViewModel model)
        {
            var r = CheckAuth(); if (r != null) return r;
            try
            {
                // Chỉ lấy các dòng người dùng có nhập SoLuongThucTe
                var coNhap = model.DanhSachChuaKiem?
                    .Where(x => x.SoLuongThucTe.HasValue)
                    .ToList();

                if (coNhap == null || coNhap.Count == 0)
                    return RedirectToAction("KiemKeTaiSan");

                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // Sinh ID phiếu an toàn
                        int soPhieu = 0;
                        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM KIEMKE", conn, tran))
                            soPhieu = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                        string idKiemKe = "KK" + soPhieu.ToString("D8");

                        // Insert phiếu kiểm kê
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO KIEMKE (ID_KiemKe, NguoiThucHienNo, NgayKiemKe, TrangThai)
                            VALUES (@id, @nguoi, GETDATE(), N'Hoàn thành')", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", idKiemKe);
                            cmd.Parameters.AddWithValue("@nguoi", Session["UserId"]?.ToString() ?? "csvc");
                            cmd.ExecuteNonQuery();
                        }

                        // Insert chi tiết — chỉ những dòng có nhập
                        int idx = 0;
                        int totalCT = 0;
                        using (var cmd2 = new SqlCommand("SELECT COUNT(*) FROM CHITIET_KIEMKE", conn, tran))
                            totalCT = Convert.ToInt32(cmd2.ExecuteScalar());

                        foreach (var item in coNhap)
                        {
                            idx++;
                            string idCT = "CT" + (totalCT + idx).ToString("D8");
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO CHITIET_KIEMKE
                                    (ID_ChiTietKK, KiemKeNo, ThietBiNo, SoLuongHeThong, SoLuongThucTe, TinhTrangThucTe, GhiChu)
                                VALUES (@id, @kk, @tb, @slht, @sl, @tt, @gc)", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@id", idCT);
                                cmd.Parameters.AddWithValue("@kk", idKiemKe);
                                cmd.Parameters.AddWithValue("@tb", item.ID_ThietBi);
                                cmd.Parameters.AddWithValue("@slht", (object)item.SoLuongHeThong);
                                cmd.Parameters.AddWithValue("@sl", item.SoLuongThucTe.Value);
                                cmd.Parameters.AddWithValue("@tt", (object)(item.TinhTrangThucTe ?? "Bình thường") ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@gc", (object)item.GhiChu ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                }
                return RedirectToAction("KiemKeTaiSan");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return Content("Lỗi: " + ex.Message);
            }
        }

        // ══ LẬP KẾ HOẠCH BẢO TRÌ ══════════════════════════════
        public ActionResult LapKeHoachBaoTri()
        {
            var r = CheckAuth(); if (r != null) return r;
            var vm = new LapKeHoachPageViewModel();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    // 1. Báo hỏng đang chờ xử lý
                    const string sqlBH = @"
                        SELECT bh.ID_BaoHong, bh.ThietBiNo, tb.TenTB,
                               ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                               ISNULL(kp.TenPhongBanKhoa,'') AS TenPhongBanKhoa,
                               bh.NguoiBaoHongNo, nd.HoTen AS HoTenNguoiBao,
                               ISNULL(bh.MoTaHong,'') AS MoTaHong,
                               bh.NgayBao,
                               ISNULL(bh.MucDoUuTien,'') AS MucDoUuTien,
                               ISNULL(bh.TrangThai,'') AS TrangThai,
                               (SELECT TOP 1 ckh.KeHoachNo FROM CHITIET_KEHOACH ckh WHERE ckh.BaoHongNo=bh.ID_BaoHong) AS KeHoachNo
                        FROM   BAOHONG_THIETBI bh
                        JOIN   THIETBI tb ON tb.ID_ThietBi=bh.ThietBiNo
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                        JOIN   NGUOIDUNG nd ON nd.ID_NguoiDung=bh.NguoiBaoHongNo
                        WHERE  bh.TrangThai=N'Chờ xử lý'
                        ORDER BY CASE bh.MucDoUuTien WHEN N'Khẩn cấp' THEN 1 WHEN N'Cao' THEN 2 WHEN N'Trung bình' THEN 3 ELSE 4 END, bh.NgayBao";
                    using (var cmd = new SqlCommand(sqlBH, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            vm.BaoHongChoCho.Add(new BaoHongViewModel
                            {
                                ID_BaoHong = rd["ID_BaoHong"].ToString(),
                                ThietBiNo = rd["ThietBiNo"].ToString(),
                                TenTB = rd["TenTB"].ToString(),
                                TenDanhMuc = rd["TenDanhMuc"].ToString(),
                                KhoaPhongBan = rd["TenPhongBanKhoa"].ToString(),
                                NguoiBaoHongNo = rd["NguoiBaoHongNo"].ToString(),
                                HoTenNguoiBao = rd["HoTenNguoiBao"].ToString(),
                                MoTaHong = rd["MoTaHong"].ToString(),
                                NgayBao = Convert.ToDateTime(rd["NgayBao"]),
                                MucDoUuTien = rd["MucDoUuTien"].ToString(),
                                TrangThai = rd["TrangThai"].ToString(),
                                KeHoachNo = rd["KeHoachNo"] == DBNull.Value ? null : rd["KeHoachNo"].ToString()
                            });

                    // 2. Danh sách kế hoạch
                    const string sqlKH = @"
                        SELECT kh.ID_KeHoach, kh.NguoiLapNo, nd.HoTen AS HoTen,
                               kh.NgayLap, kh.NgayDuKienHT, ISNULL(kh.LoaiKeHoach,N'Định kỳ') AS LoaiKeHoach,
                               ISNULL(kh.DonViThucHien,'') AS DonViThucHien,
                               kh.ChiPhiDuKien, ISNULL(kh.TrangThai,'') AS TrangThai, ISNULL(kh.GhiChu,'') AS GhiChu
                        FROM   KEHOACH_BAOTRI kh
                        JOIN   NGUOIDUNG nd ON nd.ID_NguoiDung=kh.NguoiLapNo
                        ORDER  BY kh.NgayLap DESC";
                    var khList = new List<KeHoachViewModel>();
                    using (var cmd = new SqlCommand(sqlKH, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            khList.Add(new KeHoachViewModel
                            {
                                ID_KeHoach = rd["ID_KeHoach"].ToString(),
                                NguoiLapNo = rd["NguoiLapNo"].ToString(),
                                HoTenNguoiLap = rd["HoTen"].ToString(),
                                NgayLap = Convert.ToDateTime(rd["NgayLap"]),
                                NgayDuKienHT = rd["NgayDuKienHT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayDuKienHT"]),
                                LoaiKeHoach = rd["LoaiKeHoach"].ToString(),
                                DonViThucHien = rd["DonViThucHien"].ToString(),
                                ChiPhiDuKien = rd["ChiPhiDuKien"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["ChiPhiDuKien"]),
                                TrangThai = rd["TrangThai"].ToString(),
                                GhiChu = rd["GhiChu"].ToString()
                            });

                    // 3. Chi tiết thiết bị cho từng kế hoạch
                    const string sqlCT = @"
                        SELECT ckh.ID_ChiTietKH, ckh.KeHoachNo, ckh.ThietBiNo,
                               tb.TenTB, ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                               ckh.BaoHongNo, ISNULL(bh.MoTaHong,'') AS MoTaHong,
                               CASE WHEN ckh.BaoHongNo IS NULL THEN N'Định kỳ' ELSE N'Báo hỏng' END AS NguonGoc,
                               ISNULL(ckh.GhiChuChiTiet,'') AS GhiChuChiTiet
                        FROM   CHITIET_KEHOACH ckh
                        JOIN   THIETBI tb ON tb.ID_ThietBi=ckh.ThietBiNo
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        LEFT JOIN BAOHONG_THIETBI bh ON bh.ID_BaoHong=ckh.BaoHongNo";
                    var ctDict = new Dictionary<string, List<ChiTietKeHoachViewModel>>();
                    using (var cmd = new SqlCommand(sqlCT, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            var khId = rd["KeHoachNo"].ToString();
                            if (!ctDict.ContainsKey(khId)) ctDict[khId] = new List<ChiTietKeHoachViewModel>();
                            ctDict[khId].Add(new ChiTietKeHoachViewModel
                            {
                                ID_ChiTietKH = rd["ID_ChiTietKH"].ToString(),
                                KeHoachNo = khId,
                                ThietBiNo = rd["ThietBiNo"].ToString(),
                                TenTB = rd["TenTB"].ToString(),
                                TenDanhMuc = rd["TenDanhMuc"].ToString(),
                                BaoHongNo = rd["BaoHongNo"] == DBNull.Value ? null : rd["BaoHongNo"].ToString(),
                                MoTaBaoHong = rd["MoTaHong"].ToString(),
                                NguonGoc = rd["NguonGoc"].ToString(),
                                GhiChuChiTiet = rd["GhiChuChiTiet"].ToString()
                            });
                        }
                    foreach (var kh in khList)
                        if (ctDict.ContainsKey(kh.ID_KeHoach)) kh.ChiTiet = ctDict[kh.ID_KeHoach];
                    vm.DanhSachKeHoach = khList;

                    // 4. Dropdown thiết bị
                    const string sqlTB = @"
                        SELECT tb.ID_ThietBi, tb.TenTB, ISNULL(tb.TrangThaiTB,'') AS TrangThaiTB,
                               ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                               bhp.ID_BaoHong,
                               ISNULL(bhp.MoTaHong,'') AS MoTaHong,
                               ISNULL(bhp.MucDoUuTien,'') AS MucDoUuTien
                        FROM THIETBI tb
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        LEFT JOIN (
                            SELECT src.ThietBiNo, src.ID_BaoHong, src.MoTaHong, src.MucDoUuTien,
                                   ROW_NUMBER() OVER (
                                       PARTITION BY src.ThietBiNo
                                       ORDER BY CASE src.MucDoUuTien WHEN N'Khẩn cấp' THEN 1 WHEN N'Cao' THEN 2 WHEN N'Trung bình' THEN 3 ELSE 4 END,
                                                src.NgayBao DESC
                                   ) AS RN
                            FROM BAOHONG_THIETBI src
                            WHERE src.TrangThai = N'Chờ xử lý'
                        ) bhp ON bhp.ThietBiNo=tb.ID_ThietBi AND bhp.RN=1
                        ORDER BY CASE WHEN bhp.ID_BaoHong IS NULL THEN 1 ELSE 0 END,
                                 CASE bhp.MucDoUuTien WHEN N'Khẩn cấp' THEN 1 WHEN N'Cao' THEN 2 WHEN N'Trung bình' THEN 3 ELSE 4 END,
                                 tb.TenTB";
                    using (var cmd = new SqlCommand(sqlTB, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            var bhNo = rd["ID_BaoHong"] == DBNull.Value ? null : rd["ID_BaoHong"].ToString();
                            vm.DanhSachThietBi.Add(new ThietBiDropdownViewModel
                            {
                                ID_ThietBi = rd["ID_ThietBi"].ToString(),
                                TenTB = rd["TenTB"].ToString(),
                                TrangThai = rd["TrangThaiTB"].ToString(),
                                DanhMuc = rd["TenDanhMuc"].ToString(),
                                DangBaoHong = !string.IsNullOrEmpty(bhNo),
                                BaoHongNo = bhNo,
                                MoTaBaoHong = rd["MoTaHong"].ToString(),
                                MucDoUuTienBaoHong = rd["MucDoUuTien"].ToString()
                            });
                        }
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(vm);
        }

        [HttpPost]
        public ActionResult LuuKeHoach(string idKeHoach, string loaiKeHoach, string ngayLap,
            string ngayDuKienHT, string donViThucHien, string chiPhiDuKien, string ghiChu,
            string[] thietBiIds, string[] baoHongIds, string[] ghiChuChiTiets)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            if (string.IsNullOrWhiteSpace(idKeHoach))
                return Json(new { ok = false, msg = "Vui lòng nhập ID kế hoạch." });
            if (thietBiIds == null || thietBiIds.Length == 0)
                return Json(new { ok = false, msg = "Vui lòng chọn ít nhất 1 thiết bị." });
            try
            {
                decimal? chiPhi = null;
                if (!string.IsNullOrWhiteSpace(chiPhiDuKien))
                    chiPhi = decimal.Parse(chiPhiDuKien.Replace(",", "").Replace(".", ""));

                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // Insert KEHOACH_BAOTRI
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO KEHOACH_BAOTRI
                              (ID_KeHoach,NguoiLapNo,NgayLap,NgayDuKienHT,LoaiKeHoach,DonViThucHien,ChiPhiDuKien,TrangThai,GhiChu)
                            VALUES(@ID,@NL,@NL2,@NH,@Loai,@DV,@CP,N'Chờ thực hiện',@GC)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@ID", idKeHoach);
                            cmd.Parameters.AddWithValue("@NL", CurrentUser);
                            cmd.Parameters.AddWithValue("@NL2", DateTime.Parse(ngayLap));
                            cmd.Parameters.AddWithValue("@NH", string.IsNullOrWhiteSpace(ngayDuKienHT) ? (object)DBNull.Value : DateTime.Parse(ngayDuKienHT));
                            cmd.Parameters.AddWithValue("@Loai", loaiKeHoach ?? "Định kỳ");
                            cmd.Parameters.AddWithValue("@DV", (object)donViThucHien ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CP", chiPhi.HasValue ? (object)chiPhi.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@GC", (object)ghiChu ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        // Insert CHITIET_KEHOACH
                        for (int i = 0; i < thietBiIds.Length; i++)
                        {
                            var tbId = thietBiIds[i];
                            var bhId = (baoHongIds != null && i < baoHongIds.Length && !string.IsNullOrEmpty(baoHongIds[i])) ? baoHongIds[i] : null;
                            var gc = (ghiChuChiTiets != null && i < ghiChuChiTiets.Length) ? ghiChuChiTiets[i] : null;
                            var ctId = "CT" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO CHITIET_KEHOACH(ID_ChiTietKH,KeHoachNo,ThietBiNo,BaoHongNo,GhiChuChiTiet)
                                VALUES(@ID,@KH,@TB,@BH,@GC)", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@ID", ctId);
                                cmd.Parameters.AddWithValue("@KH", idKeHoach);
                                cmd.Parameters.AddWithValue("@TB", tbId);
                                cmd.Parameters.AddWithValue("@BH", (object)bhId ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@GC", (object)gc ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                            if (bhId != null)
                                using (var cmd = new SqlCommand("UPDATE BAOHONG_THIETBI SET TrangThai=N'Đang xử lý' WHERE ID_BaoHong=@BH", conn, tran))
                                { cmd.Parameters.AddWithValue("@BH", bhId); cmd.ExecuteNonQuery(); }
                        }
                        tran.Commit();
                    }
                }
                return Json(new { ok = true, msg = "Đã lưu kế hoạch " + idKeHoach });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public ActionResult XoaKeHoach(string id)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM GHINHAN_SUA_CHUA WHERE KeHoachNo=@Id", conn))
                    { cmd.Parameters.AddWithValue("@Id", id); if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return Json(new { ok = false, msg = "Kế hoạch đã có ghi nhận, không thể xóa." }); }
                    using (var tran = conn.BeginTransaction())
                    {
                        using (var cmd = new SqlCommand("UPDATE bh SET bh.TrangThai=N'Chờ xử lý' FROM BAOHONG_THIETBI bh JOIN CHITIET_KEHOACH ckh ON ckh.BaoHongNo=bh.ID_BaoHong WHERE ckh.KeHoachNo=@Id", conn, tran))
                        { cmd.Parameters.AddWithValue("@Id", id); cmd.ExecuteNonQuery(); }
                        using (var cmd = new SqlCommand("DELETE FROM CHITIET_KEHOACH WHERE KeHoachNo=@Id", conn, tran))
                        { cmd.Parameters.AddWithValue("@Id", id); cmd.ExecuteNonQuery(); }
                        using (var cmd = new SqlCommand("DELETE FROM KEHOACH_BAOTRI WHERE ID_KeHoach=@Id", conn, tran))
                        { cmd.Parameters.AddWithValue("@Id", id); cmd.ExecuteNonQuery(); }
                        tran.Commit();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpGet]
        public JsonResult GetKeHoachDetail(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    object khData = null;
                    using (var cmd = new SqlCommand("SELECT kh.*,nd.HoTen FROM KEHOACH_BAOTRI kh JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=kh.NguoiLapNo WHERE kh.ID_KeHoach=@Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var rd = cmd.ExecuteReader())
                            if (rd.Read())
                                khData = new
                                {
                                    ID_KeHoach = rd["ID_KeHoach"].ToString(),
                                    HoTen = rd["HoTen"].ToString(),
                                    NgayLap = Convert.ToDateTime(rd["NgayLap"]).ToString("dd/MM/yyyy"),
                                    NgayDuKienHT = rd["NgayDuKienHT"] == DBNull.Value ? "" : Convert.ToDateTime(rd["NgayDuKienHT"]).ToString("dd/MM/yyyy"),
                                    LoaiKeHoach = rd["LoaiKeHoach"].ToString(),
                                    DonViThucHien = rd["DonViThucHien"]?.ToString() ?? "",
                                    ChiPhiDuKien = rd["ChiPhiDuKien"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["ChiPhiDuKien"]),
                                    TrangThai = rd["TrangThai"].ToString(),
                                    GhiChu = rd["GhiChu"]?.ToString() ?? ""
                                };
                    }
                    var ctList = new List<object>();
                    using (var cmd = new SqlCommand(@"
                        SELECT ckh.ID_ChiTietKH,ckh.ThietBiNo,tb.TenTB,ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                               ckh.BaoHongNo,ISNULL(bh.MoTaHong,'') AS MoTaHong,
                               CASE WHEN ckh.BaoHongNo IS NULL THEN N'Định kỳ' ELSE N'Báo hỏng' END AS NguonGoc,
                               ISNULL(ckh.GhiChuChiTiet,'') AS GhiChuChiTiet
                        FROM CHITIET_KEHOACH ckh
                        JOIN THIETBI tb ON tb.ID_ThietBi=ckh.ThietBiNo
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        LEFT JOIN BAOHONG_THIETBI bh ON bh.ID_BaoHong=ckh.BaoHongNo
                        WHERE ckh.KeHoachNo=@Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read())
                                ctList.Add(new
                                {
                                    ID_ChiTietKH = rd["ID_ChiTietKH"].ToString(),
                                    ThietBiNo = rd["ThietBiNo"].ToString(),
                                    TenTB = rd["TenTB"].ToString(),
                                    TenDanhMuc = rd["TenDanhMuc"].ToString(),
                                    BaoHongNo = rd["BaoHongNo"] == DBNull.Value ? "" : rd["BaoHongNo"].ToString(),
                                    MoTaBaoHong = rd["MoTaHong"].ToString(),
                                    NguonGoc = rd["NguonGoc"].ToString(),
                                    GhiChuChiTiet = rd["GhiChuChiTiet"].ToString()
                                });
                    }
                    var lsList = new List<object>();
                    using (var cmd = new SqlCommand(@"
                        SELECT gn.ID_GhiNhan, gn.NgayThucHien, ISNULL(gn.KetQua,'') AS KetQua,
                               gn.ChiPhiThucTe, ISNULL(gn.TrangThaiSauSua,'') AS TrangThaiSauSua,
                               ISNULL(gn.ChiTietKeHoachNo,'') AS ChiTietKeHoachNo,
                               ckh.ThietBiNo, ISNULL(tb.TenTB,'') AS TenTB,
                               ISNULL(ckh.BaoHongNo,'') AS BaoHongNo
                        FROM GHINHAN_SUA_CHUA gn
                        LEFT JOIN CHITIET_KEHOACH ckh ON ckh.ID_ChiTietKH = gn.ChiTietKeHoachNo
                        LEFT JOIN THIETBI tb ON tb.ID_ThietBi = ckh.ThietBiNo
                        WHERE gn.KeHoachNo = @Id
                        ORDER BY gn.NgayThucHien DESC, gn.ID_GhiNhan DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read())
                                lsList.Add(new
                                {
                                    ID_GhiNhan = rd["ID_GhiNhan"].ToString(),
                                    NgayThucHien = Convert.ToDateTime(rd["NgayThucHien"]).ToString("dd/MM/yyyy"),
                                    KetQua = rd["KetQua"].ToString(),
                                    ChiPhiThucTe = rd["ChiPhiThucTe"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["ChiPhiThucTe"]),
                                    TrangThaiSauSua = rd["TrangThaiSauSua"].ToString(),
                                    ChiTietKeHoachNo = rd["ChiTietKeHoachNo"].ToString(),
                                    ThietBiNo = rd["ThietBiNo"].ToString(),
                                    TenTB = rd["TenTB"].ToString(),
                                    BaoHongNo = rd["BaoHongNo"].ToString()
                                });
                    }
                    return Json(new { ok = true, keHoach = khData, chiTiet = ctList, lichSu = lsList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        // ══ GHI NHẬN SỬA CHỮA ══════════════════════════════════
        public ActionResult GhiNhanSuaChua()
        {
            var r = CheckAuth(); if (r != null) return r;
            var vm = new GhiNhanPageViewModel();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    // Kế hoạch chờ/đang thực hiện
                    const string sqlKH = @"
                        SELECT kh.ID_KeHoach, ISNULL(kh.LoaiKeHoach,N'Định kỳ') AS LoaiKeHoach,
                               ISNULL(kh.TrangThai,'') AS TrangThai,
                               ISNULL(kh.DonViThucHien,'') AS DonViThucHien,
                               kh.NgayLap, kh.NgayDuKienHT, kh.ChiPhiDuKien,
                               ISNULL(kh.GhiChu,'') AS GhiChu, nd.HoTen
                        FROM KEHOACH_BAOTRI kh JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=kh.NguoiLapNo
                        WHERE kh.TrangThai IN (N'Chờ thực hiện',N'Đang thực hiện')
                        ORDER BY kh.NgayLap DESC";
                    var khList = new List<KeHoachViewModel>();
                    using (var cmd = new SqlCommand(sqlKH, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            khList.Add(new KeHoachViewModel
                            {
                                ID_KeHoach = rd["ID_KeHoach"].ToString(),
                                LoaiKeHoach = rd["LoaiKeHoach"].ToString(),
                                TrangThai = rd["TrangThai"].ToString(),
                                DonViThucHien = rd["DonViThucHien"].ToString(),
                                NgayLap = Convert.ToDateTime(rd["NgayLap"]),
                                NgayDuKienHT = rd["NgayDuKienHT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayDuKienHT"]),
                                ChiPhiDuKien = rd["ChiPhiDuKien"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["ChiPhiDuKien"]),
                                GhiChu = rd["GhiChu"].ToString(),
                                HoTenNguoiLap = rd["HoTen"].ToString()
                            });
                    // Chi tiết thiết bị
                    const string sqlCT = @"
                        SELECT ckh.ID_ChiTietKH,ckh.KeHoachNo,ckh.ThietBiNo,
                               tb.TenTB,ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                               ckh.BaoHongNo,ISNULL(bh.MoTaHong,'') AS MoTaHong,
                               CASE WHEN ckh.BaoHongNo IS NULL THEN N'Định kỳ' ELSE N'Báo hỏng' END AS NguonGoc,
                               ISNULL(ckh.GhiChuChiTiet,'') AS GhiChuChiTiet
                        FROM CHITIET_KEHOACH ckh
                        JOIN THIETBI tb ON tb.ID_ThietBi=ckh.ThietBiNo
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        LEFT JOIN BAOHONG_THIETBI bh ON bh.ID_BaoHong=ckh.BaoHongNo
                        WHERE ckh.KeHoachNo IN (SELECT ID_KeHoach FROM KEHOACH_BAOTRI WHERE TrangThai IN (N'Chờ thực hiện',N'Đang thực hiện'))
                          AND NOT EXISTS (
                              SELECT 1
                              FROM GHINHAN_SUA_CHUA gn
                              WHERE gn.ChiTietKeHoachNo = ckh.ID_ChiTietKH
                          )";
                    var ctDict = new Dictionary<string, List<ChiTietKeHoachViewModel>>();
                    using (var cmd = new SqlCommand(sqlCT, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            var khId = rd["KeHoachNo"].ToString();
                            if (!ctDict.ContainsKey(khId)) ctDict[khId] = new List<ChiTietKeHoachViewModel>();
                            ctDict[khId].Add(new ChiTietKeHoachViewModel
                            {
                                ID_ChiTietKH = rd["ID_ChiTietKH"].ToString(),
                                KeHoachNo = khId,
                                ThietBiNo = rd["ThietBiNo"].ToString(),
                                TenTB = rd["TenTB"].ToString(),
                                TenDanhMuc = rd["TenDanhMuc"].ToString(),
                                BaoHongNo = rd["BaoHongNo"] == DBNull.Value ? null : rd["BaoHongNo"].ToString(),
                                MoTaBaoHong = rd["MoTaHong"].ToString(),
                                NguonGoc = rd["NguonGoc"].ToString(),
                                GhiChuChiTiet = rd["GhiChuChiTiet"].ToString()
                            });
                        }
                    foreach (var kh in khList)
                        if (ctDict.ContainsKey(kh.ID_KeHoach)) kh.ChiTiet = ctDict[kh.ID_KeHoach];

                    var khConThietBiCho = new List<KeHoachViewModel>();
                    foreach (var kh in khList)
                        if (kh.ChiTiet != null && kh.ChiTiet.Count > 0) khConThietBiCho.Add(kh);
                    vm.KeHoachChoCho = khConThietBiCho;

                    // Lịch sử ghi nhận
                    const string sqlGN = @"
                        SELECT TOP 20 gn.ID_GhiNhan, gn.KeHoachNo, ISNULL(kh.LoaiKeHoach,'') AS LoaiKeHoach,
                               gn.ChiTietKeHoachNo, ckh.ThietBiNo, ISNULL(tb.TenTB,'—') AS TenTB,
                               ckh.BaoHongNo, ISNULL(bh.MoTaHong,'') AS MoTaHong,
                               ISNULL(kh.DonViThucHien,'') AS DonViThucHien,
                               gn.NgayThucHien, ISNULL(gn.KetQua,'') AS KetQua,
                               gn.ChiPhiThucTe, ISNULL(gn.TrangThaiSauSua,'') AS TrangThaiSauSua
                        FROM GHINHAN_SUA_CHUA gn
                        JOIN KEHOACH_BAOTRI kh ON kh.ID_KeHoach=gn.KeHoachNo
                        LEFT JOIN CHITIET_KEHOACH ckh ON ckh.ID_ChiTietKH=gn.ChiTietKeHoachNo
                        LEFT JOIN THIETBI tb ON tb.ID_ThietBi=ckh.ThietBiNo
                        LEFT JOIN BAOHONG_THIETBI bh ON bh.ID_BaoHong=ckh.BaoHongNo
                        ORDER BY gn.NgayThucHien DESC";
                    using (var cmd = new SqlCommand(sqlGN, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            vm.LichSuGhiNhan.Add(new GhiNhanViewModel
                            {
                                ID_GhiNhan = rd["ID_GhiNhan"].ToString(),
                                KeHoachNo = rd["KeHoachNo"].ToString(),
                                LoaiKeHoach = rd["LoaiKeHoach"].ToString(),
                                ChiTietKeHoachNo = rd["ChiTietKeHoachNo"] == DBNull.Value ? null : rd["ChiTietKeHoachNo"].ToString(),
                                ThietBiNo = rd["ThietBiNo"] == DBNull.Value ? null : rd["ThietBiNo"].ToString(),
                                TenTB = rd["TenTB"].ToString(),
                                BaoHongNo = rd["BaoHongNo"] == DBNull.Value ? null : rd["BaoHongNo"].ToString(),
                                MoTaBaoHong = rd["MoTaHong"].ToString(),
                                DonViThucHien = rd["DonViThucHien"].ToString(),
                                NgayThucHien = Convert.ToDateTime(rd["NgayThucHien"]),
                                KetQua = rd["KetQua"].ToString(),
                                ChiPhiThucTe = rd["ChiPhiThucTe"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["ChiPhiThucTe"]),
                                TrangThaiSauSua = rd["TrangThaiSauSua"].ToString()
                            });
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(vm);
        }

        [HttpPost]
        public ActionResult LuuGhiNhan(string idGhiNhan, string keHoachNo, string chiTietKeHoachNo,
            string ngayThucHien, string ketQua, string chiPhiThucTe, string trangThaiSauSua)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            if (string.IsNullOrWhiteSpace(idGhiNhan) || string.IsNullOrWhiteSpace(keHoachNo))
                return Json(new { ok = false, msg = "Thiếu thông tin bắt buộc." });
            try
            {
                decimal? chiPhi = null;
                if (!string.IsNullOrWhiteSpace(chiPhiThucTe))
                    chiPhi = decimal.Parse(chiPhiThucTe.Replace(",", "").Replace(".", ""));

                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // 1. Insert GHINHAN_SUA_CHUA
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO GHINHAN_SUA_CHUA(ID_GhiNhan,KeHoachNo,ChiTietKeHoachNo,NgayThucHien,KetQua,ChiPhiThucTe,TrangThaiSauSua)
                            VALUES(@ID,@KH,@CTKH,@Ngay,@KQ,@CP,@TT)", conn, tran))
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
                        // 2. Cập nhật trạng thái thiết bị
                        if (!string.IsNullOrWhiteSpace(chiTietKeHoachNo) && !string.IsNullOrWhiteSpace(trangThaiSauSua))
                        {
                            string tbStatus = trangThaiSauSua.Contains("Hoạt động tốt") ? "Đang sử dụng"
                                            : trangThaiSauSua.Contains("Tạm thời") ? "Cần bảo trì"
                                            : "Báo hỏng";
                            using (var cmd = new SqlCommand("UPDATE tb SET tb.TrangThaiTB=@S FROM THIETBI tb JOIN CHITIET_KEHOACH ckh ON ckh.ThietBiNo=tb.ID_ThietBi WHERE ckh.ID_ChiTietKH=@CTKH", conn, tran))
                            { cmd.Parameters.AddWithValue("@S", tbStatus); cmd.Parameters.AddWithValue("@CTKH", chiTietKeHoachNo); cmd.ExecuteNonQuery(); }
                            // 3. Đánh dấu báo hỏng → Đã xử lý
                            using (var cmd = new SqlCommand("UPDATE bh SET bh.TrangThai=N'Đã xử lý' FROM BAOHONG_THIETBI bh JOIN CHITIET_KEHOACH ckh ON ckh.BaoHongNo=bh.ID_BaoHong WHERE ckh.ID_ChiTietKH=@CTKH AND bh.TrangThai=N'Đang xử lý'", conn, tran))
                            { cmd.Parameters.AddWithValue("@CTKH", chiTietKeHoachNo); cmd.ExecuteNonQuery(); }
                        }
                        // 4. Tự động cập nhật trạng thái kế hoạch
                        int tongCT = 0, daGN = 0;
                        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM CHITIET_KEHOACH WHERE KeHoachNo=@KH", conn, tran))
                        { cmd.Parameters.AddWithValue("@KH", keHoachNo); tongCT = Convert.ToInt32(cmd.ExecuteScalar()); }
                        using (var cmd = new SqlCommand("SELECT COUNT(DISTINCT ChiTietKeHoachNo) FROM GHINHAN_SUA_CHUA WHERE KeHoachNo=@KH AND ChiTietKeHoachNo IS NOT NULL", conn, tran))
                        { cmd.Parameters.AddWithValue("@KH", keHoachNo); daGN = Convert.ToInt32(cmd.ExecuteScalar()); }
                        string ttKH = (daGN >= tongCT && tongCT > 0) ? "Đã hoàn thành" : "Đang thực hiện";
                        using (var cmd = new SqlCommand("UPDATE KEHOACH_BAOTRI SET TrangThai=@TT WHERE ID_KeHoach=@KH", conn, tran))
                        { cmd.Parameters.AddWithValue("@TT", ttKH); cmd.Parameters.AddWithValue("@KH", keHoachNo); cmd.ExecuteNonQuery(); }

                        tran.Commit();
                        return Json(new { ok = true, trangThaiKH = ttKH, msg = "Đã lưu ghi nhận " + idGhiNhan });
                    }
                }
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // ══ PHÊ DUYỆT ĐỀ XUẤT ══════════════════════════════════
        public ActionResult PheDuyetDeXuat()
        {
            var r = CheckAuth(); if (r != null) return r;
            var choDuyet = new List<DeXuatViewModel>();
            var lichSu = new List<DeXuatViewModel>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT dx.ID_DeXuat,dx.NgayDeXuat,dx.TrangThai,dx.MoTa,dx.LyDoTuChoi,
                               nd.HoTen AS NguoiDeXuat,ISNULL(kp.TenPhongBanKhoa,N'') AS KhoaPhongBan,
                               ISNULL((SELECT SUM(ct.SoLuong*ISNULL(ct.GiaDuKien,0)) FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo=dx.ID_DeXuat),0) AS TongGia
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
                                TongGiaDuKien = Convert.ToDecimal(rd["TongGia"])
                            };
                            if (item.TrangThai == "Chờ CSVC duyệt") choDuyet.Add(item);
                            else lichSu.Add(item);
                        }
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            ViewBag.ChoDuyet = choDuyet;
            ViewBag.LichSu = lichSu;
            return View();
        }

        [HttpPost]
        public ActionResult XuLyDeXuat(string id, string action, string ghiChu)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            string trangThaiMoi = action == "duyet" ? "Chờ KHTC duyệt" : "CSVC Từ chối";
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        using (var cmd = new SqlCommand(@"UPDATE DEXUAT_MUASAM SET TrangThai=@TT, LyDoTuChoi=CASE WHEN @Act='tuchoi' THEN @GC ELSE LyDoTuChoi END, NgayDuyetCuoi=GETDATE() WHERE ID_DeXuat=@Id", conn, tran))
                        { cmd.Parameters.AddWithValue("@TT", trangThaiMoi); cmd.Parameters.AddWithValue("@Act", action ?? ""); cmd.Parameters.AddWithValue("@GC", (object)ghiChu ?? DBNull.Value); cmd.Parameters.AddWithValue("@Id", id); cmd.ExecuteNonQuery(); }

                        // Ghi lịch sử duyệt
                        using (var cmd = new SqlCommand(@"INSERT INTO LICHSUDUYET (ID_LichSu,DeXuatNo,CapDuyet,NguoiDuyetNo,ThoiGianDuyet,TrangThaiSauDuyet,GhiChu)
                            VALUES (LEFT(REPLACE(NEWID(),'-',''),10),@DX,N'CSVC',@ND,GETDATE(),@TT,@GC)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@DX", id);
                            cmd.Parameters.AddWithValue("@ND", Session["UserId"]?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@TT", trangThaiMoi);
                            cmd.Parameters.AddWithValue("@GC", (object)ghiChu ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpGet]
        public JsonResult GetChiTiet(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    var list = new List<object>();
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
                    return Json(new { ok = true, data = list }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        [HttpGet]
        public JsonResult GetThietBiById(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT ID_ThietBi, TenTB, DanhMucNo, KhoaPhongBan, NhaCCNo,
                               SoSeri, Gia, TrangThaiTB
                        FROM THIETBI
                        WHERE ID_ThietBi = @id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                var data = new
                                {
                                    ID_ThietBi = rd["ID_ThietBi"].ToString(),
                                    TenTB = rd["TenTB"].ToString(),
                                    DanhMuc = rd["DanhMucNo"]?.ToString() ?? "",
                                    KhoaPhongBan = rd["KhoaPhongBan"]?.ToString() ?? "",
                                    NhaCungCap = rd["NhaCCNo"]?.ToString() ?? "",
                                    SoSeri = rd["SoSeri"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SoSeri"]),
                                    Gia = rd["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["Gia"]),
                                    TrangThaiTB = rd["TrangThaiTB"]?.ToString() ?? ""
                                };

                                return Json(new { ok = true, data = data }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }

                return Json(new { ok = false, msg = "Không tìm thấy thiết bị" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveThietBi(ThietBiViewModel tb)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    bool isUpdate = !string.IsNullOrWhiteSpace(tb.ID_ThietBi);

                    if (!isUpdate)
                    {
                        tb.ID_ThietBi = GenerateNewThietBiID();
                    }
                    else if (!IdExists(conn, tb.ID_ThietBi))
                    {
                        return Json(new { ok = false, msg = "Không tìm thấy thiết bị để sửa" });
                    }

                    string sql = isUpdate
                        ? @"UPDATE THIETBI SET TenTB=@ten, DanhMucNo=@dm, KhoaPhongBan=@khoa,
                               NhaCCNo=@ncc, SoSeri=@seri, Gia=@gia, TrangThaiTB=@tt
                            WHERE ID_ThietBi = @id"
                        : @"INSERT INTO THIETBI (ID_ThietBi, TenTB, DanhMucNo, KhoaPhongBan, NhaCCNo, SoSeri, Gia, TrangThaiTB)
                            VALUES (@id, @ten, @dm, @khoa, @ncc, @seri, @gia, @tt)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", tb.ID_ThietBi);
                        cmd.Parameters.AddWithValue("@ten", tb.TenTB ?? "");
                        cmd.Parameters.AddWithValue("@dm", (object)tb.DanhMuc ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@khoa", (object)tb.KhoaPhongBan ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ncc", (object)tb.NhaCungCap ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@seri", (object)tb.SoSeri ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@gia", (object)tb.Gia ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@tt", tb.TrangThaiTB ?? "Mới nhập");
                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { ok = true, message = "Lưu thiết bị thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaThietBi(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = "DELETE FROM THIETBI WHERE ID_ThietBi = @id";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { ok = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GenerateNewThietBiID()
        {
            return "TB" + DateTime.Now.Ticks.ToString().Substring(10);
        }

        private bool IdExists(SqlConnection conn, string id)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM THIETBI WHERE ID_ThietBi=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
     
}

