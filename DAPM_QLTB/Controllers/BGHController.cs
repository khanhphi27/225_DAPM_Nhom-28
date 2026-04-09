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
                    const string sql = @"SELECT dx.ID_DeXuat, dx.NgayDeXuat, dx.TrangThai,
                        dx.CapDuyetHienTai, dx.MoTa, dx.LyDoTuChoi,
                        nd.HoTen AS NguoiDeXuat, kp.TenPhongBanKhoa AS KhoaPhongBan,
                        ISNULL((SELECT SUM(ct.SoLuong*ISNULL(ct.GiaDuKien,0)) FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo=dx.ID_DeXuat),0) AS TongGia
                        FROM DEXUAT_MUASAM dx
                        JOIN NGUOIDUNG nd ON nd.ID_NguoiDung = dx.NguoiDeXuatNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = nd.Khoa_BanNo
                        ORDER BY dx.NgayDeXuat DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            var item = new DeXuatViewModel {
                                ID_DeXuat = r["ID_DeXuat"].ToString(), NguoiDeXuat = r["NguoiDeXuat"].ToString(),
                                KhoaPhongBan = r["KhoaPhongBan"].ToString(), NgayDeXuat = Convert.ToDateTime(r["NgayDeXuat"]),
                                TrangThai = r["TrangThai"].ToString(), MoTa = r["MoTa"].ToString(),
                                LyDoTuChoi = r["LyDoTuChoi"].ToString(), TongGiaDuKien = Convert.ToDecimal(r["TongGia"]),
                                CapDuyetHienTai = r["CapDuyetHienTai"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["CapDuyetHienTai"])
                            };
                            if (item.TrangThai == "Chờ BGH duyệt") choDuyet.Add(item); else lichSu.Add(item);
                        }
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            ViewBag.ChoDuyet = choDuyet;
            ViewBag.LichSu   = lichSu;
            return View();
        }

        // POST: BGH/DuyetDeXuat
        [HttpPost]
        public ActionResult DuyetDeXuat(string id, string action, string ghiChu)
        {
            if (Session["UserId"] == null) return Json(new { ok = false });
            string trangThai = action == "duyet" ? "Đã duyệt" : "Từ chối";
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"UPDATE DEXUAT_MUASAM SET TrangThai=@TrangThai,
                        NguoiDuyetCuoiNo=@UserId, NgayDuyetCuoi=GETDATE(),
                        LyDoTuChoi=CASE WHEN @Action='tuchoi' THEN @GhiChu ELSE LyDoTuChoi END
                        WHERE ID_DeXuat=@Id";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                        cmd.Parameters.AddWithValue("@UserId",    Session["UserId"].ToString());
                        cmd.Parameters.AddWithValue("@Action",    action ?? "");
                        cmd.Parameters.AddWithValue("@GhiChu",    (object)ghiChu ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id",        id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // GET: BGH/ThongKeTaiSan
        public ActionResult ThongKeTaiSan()
        {
            var redirect = RequireRole(4);
            if (redirect != null) return redirect;

            var vm = new ThongKeTaiSanViewModel {
                TheoKhoa = new List<ThongKeTheoKhoaViewModel>(),
                TheoDanhMuc = new List<ThongKeTheoDanhMucViewModel>(),
                LichSuKiemKe = new List<KiemKeViewModel>()
            };
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sqlTong = @"SELECT COUNT(*) AS Tong,
                        SUM(CASE WHEN TrangThaiTB=N'Hoạt động' THEN 1 ELSE 0 END) AS HoatDong,
                        SUM(CASE WHEN TrangThaiTB=N'Bảo trì'   THEN 1 ELSE 0 END) AS BaoTri,
                        SUM(CASE WHEN TrangThaiTB=N'Hỏng'      THEN 1 ELSE 0 END) AS Hong,
                        ISNULL(SUM(Gia),0) AS TongGia FROM THIETBI";
                    using (var cmd = new SqlCommand(sqlTong, conn))
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) { vm.TongThietBi = Convert.ToInt32(r["Tong"]); vm.HoatDong = Convert.ToInt32(r["HoatDong"]); vm.BaoTri = Convert.ToInt32(r["BaoTri"]); vm.Hong = Convert.ToInt32(r["Hong"]); vm.TongGiaTri = Convert.ToDecimal(r["TongGia"]); }

                    const string sqlKhoa = @"SELECT kp.TenPhongBanKhoa, COUNT(tb.ID_ThietBi) AS Tong,
                        SUM(CASE WHEN tb.TrangThaiTB=N'Hoạt động' THEN 1 ELSE 0 END) AS HoatDong,
                        SUM(CASE WHEN tb.TrangThaiTB=N'Bảo trì'   THEN 1 ELSE 0 END) AS BaoTri,
                        SUM(CASE WHEN tb.TrangThaiTB=N'Hỏng'      THEN 1 ELSE 0 END) AS Hong,
                        ISNULL(SUM(tb.Gia),0) AS TongGia
                        FROM THIETBI tb JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                        GROUP BY kp.TenPhongBanKhoa ORDER BY Tong DESC";
                    using (var cmd = new SqlCommand(sqlKhoa, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.TheoKhoa.Add(new ThongKeTheoKhoaViewModel { TenKhoa = r["TenPhongBanKhoa"].ToString(), Tong = Convert.ToInt32(r["Tong"]), HoatDong = Convert.ToInt32(r["HoatDong"]), BaoTri = Convert.ToInt32(r["BaoTri"]), Hong = Convert.ToInt32(r["Hong"]), TongGia = Convert.ToDecimal(r["TongGia"]) });

                    const string sqlDM = @"SELECT dm.TenDanhMuc, COUNT(tb.ID_ThietBi) AS SoLuong
                        FROM THIETBI tb JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                        GROUP BY dm.TenDanhMuc ORDER BY SoLuong DESC";
                    using (var cmd = new SqlCommand(sqlDM, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.TheoDanhMuc.Add(new ThongKeTheoDanhMucViewModel { TenDanhMuc = r["TenDanhMuc"].ToString(), SoLuong = Convert.ToInt32(r["SoLuong"]) });

                    const string sqlKK = @"SELECT kk.ID_KiemKe, kk.NgayKiemKe, kk.TrangThai, kk.GhiChu,
                        nd.HoTen AS NguoiThucHien,
                        (SELECT COUNT(*) FROM CHITIET_KIEMKE ck WHERE ck.KiemKeNo=kk.ID_KiemKe) AS TongTB
                        FROM KIEMKE kk JOIN NGUOIDUNG nd ON nd.ID_NguoiDung=kk.NguoiThucHienNo
                        ORDER BY kk.NgayKiemKe DESC";
                    using (var cmd = new SqlCommand(sqlKK, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.LichSuKiemKe.Add(new KiemKeViewModel { ID_KiemKe = r["ID_KiemKe"].ToString(), NgayKiemKe = Convert.ToDateTime(r["NgayKiemKe"]), NguoiThucHien = r["NguoiThucHien"].ToString(), TrangThai = r["TrangThai"].ToString(), GhiChu = r["GhiChu"].ToString(), TongThietBiKK = Convert.ToInt32(r["TongTB"]) });
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
                        WHERE tb.TrangThaiTB<>N'Hoạt động' ORDER BY tb.TrangThaiTB, bh.NgayBao DESC";
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
                        WHERE tb.TrangThaiTB=N'Hoạt động' ORDER BY tb.ID_ThietBi";
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
