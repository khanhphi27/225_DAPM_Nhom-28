using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using QLTB.Models;

namespace QLTB.Controllers
{
    public class KHTCController : Controller
    {
        private ActionResult RequireRole(int requiredRole)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != requiredRole)
                return RedirectToAction("Index", "Home");
            return null;
        }

        // GET: KHTC/Index
        public ActionResult Index()
        {
            var redirect = RequireRole(3);
            if (redirect != null) return redirect;

            var vm = new KHTCDashboardViewModel();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sqlFinance = @"SELECT
                        (SELECT ISNULL(SUM(Gia),0) FROM THIETBI) AS TongGiaTri,
                        (SELECT ISNULL(SUM(ChiPhiThucTe),0) FROM GHINHAN_SUA_CHUA) AS TongChiPhiSuaChua,
                        (SELECT COUNT(*) FROM DEXUAT_MUASAM WHERE TrangThai=N'Chờ KHTC duyệt') AS ChoDuyet";
                    using (var cmd = new SqlCommand(sqlFinance, conn))
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) { vm.TongGiaTri = Convert.ToDecimal(r["TongGiaTri"]); vm.TongSuaChua = Convert.ToDecimal(r["TongChiPhiSuaChua"]); vm.ChoDuyet = Convert.ToInt32(r["ChoDuyet"]); }

                    const string sqlHD = @"SELECT TOP 5 dx.ID_DeXuat, dx.NgayDeXuat, dx.TrangThai, nd.HoTen AS NguoiDeXuat
                        FROM DEXUAT_MUASAM dx JOIN NGUOIDUNG nd ON dx.NguoiDeXuatNo=nd.ID_NguoiDung
                        ORDER BY dx.NgayDeXuat DESC";
                    using (var cmd = new SqlCommand(sqlHD, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.HoatDongGanDay.Add(new DeXuatViewModel {
                                ID_DeXuat = r["ID_DeXuat"].ToString(), NguoiDeXuat = r["NguoiDeXuat"].ToString(),
                                NgayDeXuat = Convert.ToDateTime(r["NgayDeXuat"]), TrangThai = r["TrangThai"].ToString()
                            });
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(vm);
        }

        // GET: KHTC/PheDuyetNganSach
        public ActionResult PheDuyetNganSach()
        {
            var redirect = RequireRole(3);
            if (redirect != null) return redirect;

            var list = new List<DeXuatViewModel>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"SELECT dx.ID_DeXuat, dx.NgayDeXuat, dx.TrangThai, dx.MoTa, nd.HoTen AS NguoiDeXuat
                        FROM DEXUAT_MUASAM dx JOIN NGUOIDUNG nd ON dx.NguoiDeXuatNo=nd.ID_NguoiDung
                        WHERE dx.TrangThai=N'Chờ KHTC duyệt' ORDER BY dx.NgayDeXuat DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new DeXuatViewModel {
                                ID_DeXuat = r["ID_DeXuat"].ToString(), NguoiDeXuat = r["NguoiDeXuat"].ToString(),
                                NgayDeXuat = Convert.ToDateTime(r["NgayDeXuat"]), TrangThai = r["TrangThai"].ToString(),
                                MoTa = r.IsDBNull(r.GetOrdinal("MoTa")) ? "" : r["MoTa"].ToString()
                            });
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(list);
        }

        // GET: KHTC/BaoCaoTaiSan
        public ActionResult BaoCaoTaiSan()
        {
            var redirect = RequireRole(3);
            if (redirect != null) return redirect;

            var list = new List<BaoCaoTaiSanViewModel>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM View_BaoCaoTaiChinh", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new BaoCaoTaiSanViewModel {
                                ID_ThietBi = r["ID_ThietBi"].ToString(), TenTB = r["TenTB"].ToString(),
                                TenDanhMuc = r.IsDBNull(r.GetOrdinal("TenDanhMuc")) ? "Chưa phân loại" : r["TenDanhMuc"].ToString(),
                                TenPhongBanKhoa = r.IsDBNull(r.GetOrdinal("TenPhongBanKhoa")) ? "Chưa cấp phát" : r["TenPhongBanKhoa"].ToString(),
                                Gia = r.IsDBNull(r.GetOrdinal("Gia")) ? 0 : Convert.ToDecimal(r["Gia"]),
                                TrangThaiTB = r["TrangThaiTB"].ToString(),
                                TongChiPhiSuaChua = r.IsDBNull(r.GetOrdinal("TongChiPhiSuaChua")) ? 0 : Convert.ToDecimal(r["TongChiPhiSuaChua"])
                            });
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(list);
        }

        // GET: KHTC/QuanLyChiPhi
        public ActionResult QuanLyChiPhi()
        {
            var redirect = RequireRole(3);
            if (redirect != null) return redirect;

            var list = new List<ChiPhiViewModel>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"SELECT gn.ID_GhiNhan, gn.NgayThucHien, gn.ChiPhiThucTe, gn.KetQua, kh.DonViThucHien
                        FROM GHINHAN_SUA_CHUA gn JOIN KEHOACH_BAOTRI kh ON gn.KeHoachNo=kh.ID_KeHoach
                        ORDER BY gn.NgayThucHien DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new ChiPhiViewModel {
                                ID_GhiNhan = r["ID_GhiNhan"].ToString(),
                                NgayThucHien = Convert.ToDateTime(r["NgayThucHien"]),
                                ChiPhiThucTe = r.IsDBNull(r.GetOrdinal("ChiPhiThucTe")) ? 0 : Convert.ToDecimal(r["ChiPhiThucTe"]),
                                KetQua = r.IsDBNull(r.GetOrdinal("KetQua")) ? "" : r["KetQua"].ToString(),
                                DonViThucHien = r.IsDBNull(r.GetOrdinal("DonViThucHien")) ? "Nội bộ" : r["DonViThucHien"].ToString()
                            });
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View(list);
        }

        // POST: KHTC/XuLyNganSach
        [HttpPost]
        public ActionResult XuLyNganSach(string id, string action, string ghiChu)
        {
            if (Session["UserId"] == null) return Json(new { ok = false });
            string trangThai = action == "duyet" ? "Chờ BGH duyệt" : "KHTC Từ chối";
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"UPDATE DEXUAT_MUASAM SET TrangThai=@TrangThai, LyDoTuChoi=@GhiChu, NgayDuyetCuoi=GETDATE() WHERE ID_DeXuat=@Id";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                        cmd.Parameters.AddWithValue("@GhiChu",    (object)ghiChu ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id",        id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // POST: KHTC/CapNhatChiPhi
        [HttpPost]
        public ActionResult CapNhatChiPhi(string id, decimal chiPhiMoi)
        {
            var redirect = RequireRole(3);
            if (redirect != null) return Json(new { ok = false, msg = "Không có quyền." });
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("UPDATE GHINHAN_SUA_CHUA SET ChiPhiThucTe=@ChiPhiMoi WHERE ID_GhiNhan=@Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@ChiPhiMoi", chiPhiMoi);
                        cmd.Parameters.AddWithValue("@Id",        id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // GET: KHTC/GetDeXuatDetail
        [HttpGet]
        public JsonResult GetDeXuatDetail(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"SELECT ct.TenThietBiDeXuat, ct.SoLuong, ct.GiaDuKien, ct.DonViTinh, dm.TenDanhMuc
                        FROM CHITIET_DEXUAT ct LEFT JOIN DANHMUC dm ON ct.DanhMucNo=dm.ID_DanhMuc
                        WHERE ct.DeXuatNo=@Id";
                    var items = new List<object>();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                items.Add(new { Ten = r["TenThietBiDeXuat"].ToString(), SoLuong = r["SoLuong"], Gia = r["GiaDuKien"], DVT = r["DonViTinh"].ToString(), DanhMuc = r["TenDanhMuc"].ToString() });
                    }
                    return Json(new { ok = true, data = items }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
    }
}
