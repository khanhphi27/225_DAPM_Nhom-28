using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using QLTB.Models;

namespace QLTB.Controllers
{
    public class BGHController : Controller
    {
        private ActionResult RequireRole(int requiredRole)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != requiredRole)
                return RedirectToAction("Index", "Home");
            return null;
        }

        // GET: BGH/Index
        public ActionResult Index()
        {
            var redirect = RequireRole(4);
            if (redirect != null) return redirect;

            var vm = new BGHDashboardViewModel { HoatDongGanDay = new List<HoatDongGanDayViewModel>() };
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sqlTB = @"SELECT COUNT(*) AS Tong,
                        SUM(CASE WHEN TrangThaiTB=N'Hoạt động' THEN 1 ELSE 0 END) AS HoatDong,
                        SUM(CASE WHEN TrangThaiTB=N'Bảo trì'   THEN 1 ELSE 0 END) AS BaoTri,
                        SUM(CASE WHEN TrangThaiTB=N'Hỏng'      THEN 1 ELSE 0 END) AS Hong
                        FROM THIETBI";
                    using (var cmd = new SqlCommand(sqlTB, conn))
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) { vm.TongThietBi = Convert.ToInt32(r["Tong"]); vm.HoatDong = Convert.ToInt32(r["HoatDong"]); vm.BaoTri = Convert.ToInt32(r["BaoTri"]); vm.Hong = Convert.ToInt32(r["Hong"]); }

                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM DEXUAT_MUASAM WHERE TrangThai=N'Chờ BGH duyệt'", conn))
                        vm.ChoDeXuatDuyet = (int)cmd.ExecuteScalar();

                    const string sqlHD = @"SELECT TOP 10 dx.ID_DeXuat, dx.MoTa, dx.NgayDeXuat, dx.TrangThai,
                        nd.HoTen AS NguoiDeXuat, kp.TenPhongBanKhoa AS KhoaPhongBan
                        FROM DEXUAT_MUASAM dx
                        JOIN NGUOIDUNG nd ON nd.ID_NguoiDung = dx.NguoiDeXuatNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = nd.Khoa_BanNo
                        ORDER BY dx.NgayDeXuat DESC";
                    using (var cmd = new SqlCommand(sqlHD, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.HoatDongGanDay.Add(new HoatDongGanDayViewModel {
                                ID_DeXuat = r["ID_DeXuat"].ToString(), MoTa = r["MoTa"].ToString(),
                                NgayDeXuat = Convert.ToDateTime(r["NgayDeXuat"]), TrangThai = r["TrangThai"].ToString(),
                                NguoiDeXuat = r["NguoiDeXuat"].ToString(), KhoaPhongBan = r["KhoaPhongBan"].ToString()
                            });
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(vm);
        }

        // GET: BGH/GetChiTietDeXuat
        [HttpGet]
        public JsonResult GetChiTietDeXuat(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = "SELECT TenThietBiDeXuat, SoLuong, GiaDuKien, DonViTinh FROM CHITIET_DEXUAT WHERE DeXuatNo=@Id";
                    var list = new System.Collections.Generic.List<object>();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                list.Add(new {
                                    Ten     = r["TenThietBiDeXuat"].ToString(),
                                    SoLuong = r["SoLuong"]   == DBNull.Value ? 0 : Convert.ToInt32(r["SoLuong"]),
                                    Gia     = r["GiaDuKien"] == DBNull.Value ? 0m : Convert.ToDecimal(r["GiaDuKien"]),
                                    DVT     = r["DonViTinh"] == DBNull.Value ? "" : r["DonViTinh"].ToString()
                                });
                    }
                    return Json(new { ok = true, data = list }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        // GET: BGH/XetDuyetYeuCau
        public ActionResult XetDuyetYeuCau()
        {
            var redirect = RequireRole(4);
            if (redirect != null) return redirect;

            var choDuyet = new List<DeXuatViewModel>();
            var lichSu   = new List<DeXuatViewModel>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT dx.ID_DeXuat, dx.NgayDeXuat, dx.TrangThai, dx.MoTa, dx.LyDoTuChoi,
                               nd.HoTen AS NguoiDeXuat,
                               ISNULL(kp.TenPhongBanKhoa, N'') AS KhoaPhongBan,
                               ISNULL((SELECT SUM(ct.SoLuong*ISNULL(ct.GiaDuKien,0))
                                       FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo=dx.ID_DeXuat),0) AS TongGia
                        FROM   DEXUAT_MUASAM dx
                        JOIN   NGUOIDUNG nd ON nd.ID_NguoiDung = dx.NguoiDeXuatNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = nd.Khoa_BanNo
                        ORDER  BY dx.NgayDeXuat DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            var item = new DeXuatViewModel {
                                ID_DeXuat     = rd["ID_DeXuat"].ToString(),
                                NguoiDeXuat   = rd["NguoiDeXuat"].ToString(),
                                KhoaPhongBan  = rd["KhoaPhongBan"].ToString(),
                                NgayDeXuat    = Convert.ToDateTime(rd["NgayDeXuat"]),
                                TrangThai     = rd["TrangThai"].ToString(),
                                MoTa          = rd["MoTa"]       == DBNull.Value ? "" : rd["MoTa"].ToString(),
                                LyDoTuChoi    = rd["LyDoTuChoi"] == DBNull.Value ? "" : rd["LyDoTuChoi"].ToString(),
                                TongGiaDuKien = Convert.ToDecimal(rd["TongGia"])
                            };
                            if (item.TrangThai == "Chờ BGH duyệt") choDuyet.Add(item);
                            else lichSu.Add(item);
                        }
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            ViewBag.ChoDuyet = choDuyet;
            ViewBag.LichSu   = lichSu;
            return View();
        }

        // POST: BGH/DuyetDeXuat — bước cuối, gửi thông báo cho TẤT CẢ actors
        [HttpPost]
        public ActionResult DuyetDeXuat(string id, string action, string ghiChu)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            string trangThai = action == "duyet" ? "Đã duyệt" : "BGH Từ chối";
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // Cập nhật trạng thái cuối
                        using (var cmd = new SqlCommand(
                            @"UPDATE DEXUAT_MUASAM
                              SET TrangThai      = @TrangThai,
                                  NguoiDuyetCuoiNo = @UserId,
                                  NgayDuyetCuoi  = GETDATE(),
                                  LyDoTuChoi     = CASE WHEN @Action='tuchoi' THEN @GhiChu ELSE LyDoTuChoi END
                              WHERE ID_DeXuat = @Id", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                            cmd.Parameters.AddWithValue("@UserId",    Session["UserId"].ToString());
                            cmd.Parameters.AddWithValue("@Action",    action ?? "");
                            cmd.Parameters.AddWithValue("@GhiChu",    (object)ghiChu ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Id",        id);
                            cmd.ExecuteNonQuery();
                        }

                        // Lấy người đề xuất
                        string nguoiDX = null;
                        using (var cmd = new SqlCommand("SELECT NguoiDeXuatNo FROM DEXUAT_MUASAM WHERE ID_DeXuat=@Id", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            var v = cmd.ExecuteScalar();
                            if (v != null) nguoiDX = v.ToString();
                        }

                        string tieuDe  = action == "duyet"
                            ? "✅ Đề xuất mua sắm đã được BGH phê duyệt"
                            : "❌ Đề xuất mua sắm bị BGH từ chối";
                        string noiDung = action == "duyet"
                            ? "Ban Giám Hiệu đã phê duyệt đề xuất (Mã: " + id + "). Quy trình hoàn tất."
                            : "Ban Giám Hiệu đã từ chối đề xuất (Mã: " + id + "). Lý do: " + ghiChu;
                        string loai = action == "duyet" ? "approved" : "rejected";

                        const string sqlVaiTro = @"INSERT INTO THONGBAO (ID_ThongBao,NguoiNhanNo,TieuDe,NoiDung,NgayTao,LoaiThongBao,DaDoc)
                            SELECT NEWID(),vn.NguoiDungNo,@TieuDe,@NoiDung,GETDATE(),@Loai,0
                            FROM VAITRO_NGUOIDUNG vn WHERE vn.VaiTroNo=@VaiTro";
                        const string sqlUser = @"INSERT INTO THONGBAO (ID_ThongBao,NguoiNhanNo,TieuDe,NoiDung,NgayTao,LoaiThongBao,DaDoc)
                            VALUES (NEWID(),@NguoiDung,@TieuDe,@NoiDung,GETDATE(),@Loai,0)";

                        try
                        {
                            // Gửi cho Trưởng Khoa (người đề xuất)
                            if (nguoiDX != null)
                                using (var cmd = new SqlCommand(sqlUser, conn, tran))
                                {
                                    cmd.Parameters.AddWithValue("@NguoiDung", nguoiDX);
                                    cmd.Parameters.AddWithValue("@TieuDe",    tieuDe);
                                    cmd.Parameters.AddWithValue("@NoiDung",   noiDung);
                                    cmd.Parameters.AddWithValue("@Loai",      loai);
                                    cmd.ExecuteNonQuery();
                                }
                            // Gửi cho toàn bộ CSVC
                            using (var cmd = new SqlCommand(sqlVaiTro, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@VaiTro",  "VT_CSVC");
                                cmd.Parameters.AddWithValue("@TieuDe",  tieuDe);
                                cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                                cmd.Parameters.AddWithValue("@Loai",    loai);
                                cmd.ExecuteNonQuery();
                            }
                            // Gửi cho toàn bộ KHTC
                            using (var cmd = new SqlCommand(sqlVaiTro, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@VaiTro",  "VT_KHTC");
                                cmd.Parameters.AddWithValue("@TieuDe",  tieuDe);
                                cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                                cmd.Parameters.AddWithValue("@Loai",    loai);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch { /* bảng THONGBAO chưa tạo thì bỏ qua */ }

                        // Ghi lịch sử duyệt BGH
                        try
                        {
                            using (var cmd = new SqlCommand(@"INSERT INTO LICHSUDUYET (ID_LichSu,DeXuatNo,CapDuyet,NguoiDuyetNo,ThoiGianDuyet,TrangThaiSauDuyet,GhiChu)
                                VALUES (LEFT(REPLACE(NEWID(),'-',''),10),@DX,N'BGH',@ND,GETDATE(),@TT,@GC)", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@DX", id);
                                cmd.Parameters.AddWithValue("@ND", Session["UserId"]?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@TT", trangThai);
                                cmd.Parameters.AddWithValue("@GC", (object)ghiChu ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch { }

                        tran.Commit();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // GET: BGH/DebugTrangThai - xem giá trị TrangThaiTB thực tế trong DB
        [HttpGet]
        public JsonResult DebugTrangThai()
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    var list = new List<object>();
                    using (var cmd = new SqlCommand("SELECT TrangThaiTB, COUNT(*) AS SoLuong FROM THIETBI GROUP BY TrangThaiTB", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new { TrangThai = r["TrangThaiTB"]?.ToString(), SoLuong = r["SoLuong"] });
                    return Json(new { ok = true, data = list }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        // GET: BGH/ThongKeTaiSan
        public ActionResult ThongKeTaiSan()
        {
            var redirect = RequireRole(4);
            if (redirect != null) return redirect;

            var vm = new ThongKeTaiSanViewModel {
                TheoKhoa     = new List<ThongKeTheoKhoaViewModel>(),
                TheoDanhMuc  = new List<ThongKeTheoDanhMucViewModel>(),
                LichSuKiemKe = new List<KiemKeViewModel>(),
                CanChuY      = new List<ThietBiCanChuYViewModel>()
            };
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    // 1. Tổng quan - đếm theo từng giá trị TrangThaiTB thực tế
                    const string sqlTong = @"
                        SELECT COUNT(*) AS Tong,
                            ISNULL(SUM(Gia),0) AS TongGia
                        FROM THIETBI";
                    using (var cmd = new SqlCommand(sqlTong, conn))
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) {
                            vm.TongThietBi = Convert.ToInt32(r["Tong"]);
                            vm.TongGiaTri  = Convert.ToDecimal(r["TongGia"]);
                        }

                    // Đếm từng trạng thái - DB dùng 'Đang sử dụng', 'Bảo trì', 'Hỏng'
                    const string sqlDemTT = @"
                        SELECT TrangThaiTB, COUNT(*) AS SoLuong
                        FROM THIETBI
                        GROUP BY TrangThaiTB";
                    using (var cmd = new SqlCommand(sqlDemTT, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            string tt = (r["TrangThaiTB"]?.ToString() ?? "").Trim();
                            int sl = Convert.ToInt32(r["SoLuong"]);
                            if (tt == "Đang sử dụng")
                                vm.HoatDong += sl;
                            else if (tt == "Cần bảo trì")
                                vm.BaoTri += sl;
                            else if (tt == "Báo hỏng")
                                vm.Hong += sl;
                        }

                    // 2. Tổng chi phí bảo trì thực tế
                    using (var cmd = new SqlCommand("SELECT ISNULL(SUM(ChiPhiThucTe),0) FROM GHINHAN_SUA_CHUA", conn))
                        vm.ChiPhiBaoTri = Convert.ToDecimal(cmd.ExecuteScalar());

                    // 3. Số báo hỏng
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM BAOHONG_THIETBI", conn))
                        vm.TongBaoHong = Convert.ToInt32(cmd.ExecuteScalar());

                    // 4. Theo khoa
                    const string sqlKhoa = @"
                        SELECT kp.TenPhongBanKhoa,
                            COUNT(tb.ID_ThietBi) AS Tong,
                            SUM(CASE WHEN tb.TrangThaiTB=N'Đang sử dụng' THEN 1 ELSE 0 END) AS HoatDong,
                            SUM(CASE WHEN tb.TrangThaiTB=N'Cần bảo trì'  THEN 1 ELSE 0 END) AS BaoTri,
                            SUM(CASE WHEN tb.TrangThaiTB=N'Báo hỏng'     THEN 1 ELSE 0 END) AS Hong,
                            ISNULL(SUM(tb.Gia),0) AS TongGia
                        FROM THIETBI tb
                        JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = tb.KhoaPhongBan
                        GROUP BY kp.TenPhongBanKhoa
                        ORDER BY Tong DESC";
                    using (var cmd = new SqlCommand(sqlKhoa, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.TheoKhoa.Add(new ThongKeTheoKhoaViewModel {
                                TenKhoa  = r["TenPhongBanKhoa"].ToString(),
                                Tong     = Convert.ToInt32(r["Tong"]),
                                HoatDong = Convert.ToInt32(r["HoatDong"]),
                                BaoTri   = Convert.ToInt32(r["BaoTri"]),
                                Hong     = Convert.ToInt32(r["Hong"]),
                                TongGia  = Convert.ToDecimal(r["TongGia"])
                            });

                    // 5. Theo danh mục
                    const string sqlDM = @"
                        SELECT dm.TenDanhMuc, COUNT(tb.ID_ThietBi) AS SoLuong
                        FROM THIETBI tb
                        JOIN DANHMUC dm ON dm.ID_DanhMuc = tb.DanhMucNo
                        GROUP BY dm.TenDanhMuc
                        ORDER BY SoLuong DESC";
                    using (var cmd = new SqlCommand(sqlDM, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.TheoDanhMuc.Add(new ThongKeTheoDanhMucViewModel {
                                TenDanhMuc = r["TenDanhMuc"].ToString(),
                                SoLuong    = Convert.ToInt32(r["SoLuong"])
                            });

                    // 6. Thiết bị cần chú ý (hỏng / bảo trì)
                    const string sqlCanChuY = @"
                        SELECT TOP 20 tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB, tb.Gia,
                            ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                            ISNULL(kp.TenPhongBanKhoa,'') AS KhoaPhongBan,
                            bh.MoTaHong, bh.NgayBao
                        FROM THIETBI tb
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc = tb.DanhMucNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = tb.KhoaPhongBan
                        OUTER APPLY (
                            SELECT TOP 1 MoTaHong, NgayBao
                            FROM BAOHONG_THIETBI
                            WHERE ThietBiNo = tb.ID_ThietBi
                            ORDER BY NgayBao DESC
                        ) bh
                        WHERE tb.TrangThaiTB <> N'Đang sử dụng'
                        ORDER BY tb.TrangThaiTB, bh.NgayBao DESC";
                    using (var cmd = new SqlCommand(sqlCanChuY, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.CanChuY.Add(new ThietBiCanChuYViewModel {
                                ID_ThietBi   = r["ID_ThietBi"].ToString(),
                                TenTB        = r["TenTB"].ToString(),
                                TrangThaiTB  = r["TrangThaiTB"].ToString(),
                                TenDanhMuc   = r["TenDanhMuc"].ToString(),
                                KhoaPhongBan = r["KhoaPhongBan"].ToString(),
                                Gia          = r["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Gia"]),
                                MoTaHong     = r["MoTaHong"] == DBNull.Value ? "" : r["MoTaHong"].ToString(),
                                NgayBaoHong  = r["NgayBao"]  == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["NgayBao"])
                            });

                    // 7. Lịch sử kiểm kê
                    try
                    {
                        const string sqlKK = @"
                            SELECT kk.ID_KiemKe, kk.NgayKiemKe, kk.TrangThai, kk.GhiChu,
                                nd.HoTen AS NguoiThucHien,
                                (SELECT COUNT(*) FROM CHITIET_KIEMKE ck WHERE ck.KiemKeNo=kk.ID_KiemKe) AS TongTB
                            FROM KIEMKE kk
                            JOIN NGUOIDUNG nd ON nd.ID_NguoiDung = kk.NguoiThucHienNo
                            ORDER BY kk.NgayKiemKe DESC";
                        using (var cmd = new SqlCommand(sqlKK, conn))
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                vm.LichSuKiemKe.Add(new KiemKeViewModel {
                                    ID_KiemKe     = r["ID_KiemKe"].ToString(),
                                    NgayKiemKe    = Convert.ToDateTime(r["NgayKiemKe"]),
                                    NguoiThucHien = r["NguoiThucHien"].ToString(),
                                    TrangThai     = r["TrangThai"].ToString(),
                                    GhiChu        = r["GhiChu"].ToString(),
                                    TongThietBiKK = Convert.ToInt32(r["TongTB"])
                                });
                    }
                    catch { /* bảng KIEMKE có thể chưa có dữ liệu */ }
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(vm);
        }

        // GET: BGH/TheoDoi
        public ActionResult TheoDoi()
        {
            var redirect = RequireRole(4);
            if (redirect != null) return redirect;

            var canChuY  = new List<TheDoiThietBiViewModel>();
            var hoatDong = new List<TheDoiThietBiViewModel>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sqlCan = @"SELECT tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB, tb.Gia,
                        dm.TenDanhMuc, kp.TenPhongBanKhoa AS KhoaPhongBan,
                        ncc.TenNhaCC AS NhaCungCap, p.TenPhong AS Phong,
                        bh.MoTaHong, bh.NgayBao AS NgayBaoHong, nd.HoTen AS NguoiBaoHong
                        FROM THIETBI tb
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                        LEFT JOIN NHACUNGCAP ncc ON ncc.ID_NhaCC=tb.NhaCCNo
                        LEFT JOIN PHONG_THIETBI ptb ON ptb.ThietBiNo=tb.ID_ThietBi
                        LEFT JOIN PHONG p ON p.ID_Phong=ptb.PhongNo
                        OUTER APPLY (SELECT TOP 1 MoTaHong, NgayBao, NguoiBaoHongNo FROM BAOHONG_THIETBI WHERE ThietBiNo=tb.ID_ThietBi ORDER BY NgayBao DESC) bh
                        LEFT JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=bh.NguoiBaoHongNo
                        WHERE tb.TrangThaiTB<>N'Đang sử dụng' ORDER BY tb.TrangThaiTB, bh.NgayBao DESC";
                    using (var cmd = new SqlCommand(sqlCan, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) canChuY.Add(ReadThietBi(r));

                    const string sqlOk = @"SELECT TOP 20 tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB, tb.Gia,
                        dm.TenDanhMuc, kp.TenPhongBanKhoa AS KhoaPhongBan,
                        ncc.TenNhaCC AS NhaCungCap, p.TenPhong AS Phong,
                        NULL AS MoTaHong, NULL AS NgayBaoHong, NULL AS NguoiBaoHong
                        FROM THIETBI tb
                        LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                        LEFT JOIN NHACUNGCAP ncc ON ncc.ID_NhaCC=tb.NhaCCNo
                        LEFT JOIN PHONG_THIETBI ptb ON ptb.ThietBiNo=tb.ID_ThietBi
                        LEFT JOIN PHONG p ON p.ID_Phong=ptb.PhongNo
                        WHERE tb.TrangThaiTB=N'Đang sử dụng' ORDER BY tb.ID_ThietBi";
                    using (var cmd = new SqlCommand(sqlOk, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) hoatDong.Add(ReadThietBi(r));
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            ViewBag.CanChuY  = canChuY;
            ViewBag.HoatDong = hoatDong;
            return View();
        }

        private TheDoiThietBiViewModel ReadThietBi(SqlDataReader r)
        {
            return new TheDoiThietBiViewModel {
                ID_ThietBi = r["ID_ThietBi"].ToString(), TenTB = r["TenTB"].ToString(),
                TrangThaiTB = r["TrangThaiTB"].ToString(), TenDanhMuc = r["TenDanhMuc"].ToString(),
                KhoaPhongBan = r["KhoaPhongBan"].ToString(),
                Gia = r["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Gia"]),
                NhaCungCap = r["NhaCungCap"].ToString(), Phong = r["Phong"].ToString(),
                MoTaHong = r["MoTaHong"] == DBNull.Value ? null : r["MoTaHong"].ToString(),
                NgayBaoHong = r["NgayBaoHong"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["NgayBaoHong"]),
                NguoiBaoHong = r["NguoiBaoHong"] == DBNull.Value ? null : r["NguoiBaoHong"].ToString()
            };
        }
    }
}
