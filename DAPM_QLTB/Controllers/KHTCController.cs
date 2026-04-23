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

        // POST: KHTC/XuLyNganSach — KHTC duyệt → Chờ BGH, từ chối → KHTC Từ chối
        [HttpPost]
        public ActionResult XuLyNganSach(string id, string action, string ghiChu)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            string trangThai = action == "duyet" ? "Chờ BGH duyệt" : "KHTC Từ chối";
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // Cập nhật trạng thái
                        using (var cmd = new SqlCommand(
                            @"UPDATE DEXUAT_MUASAM
                              SET TrangThai    = @TrangThai,
                                  LyDoTuChoi   = CASE WHEN @Action='tuchoi' THEN @GhiChu ELSE LyDoTuChoi END,
                                  NgayDuyetCuoi = GETDATE()
                              WHERE ID_DeXuat = @Id", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@TrangThai", trangThai);
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

                        const string sqlVaiTro = @"INSERT INTO THONGBAO (ID_ThongBao,NguoiNhanNo,TieuDe,NoiDung,NgayTao,LoaiThongBao,DaDoc)
                            SELECT NEWID(),vn.NguoiDungNo,@TieuDe,@NoiDung,GETDATE(),@Loai,0
                            FROM VAITRO_NGUOIDUNG vn WHERE vn.VaiTroNo=@VaiTro";
                        const string sqlUser = @"INSERT INTO THONGBAO (ID_ThongBao,NguoiNhanNo,TieuDe,NoiDung,NgayTao,LoaiThongBao,DaDoc)
                            VALUES (NEWID(),@NguoiDung,@TieuDe,@NoiDung,GETDATE(),@Loai,0)";

                        if (action == "duyet")
                        {
                            try
                            {
                                using (var cmd = new SqlCommand(sqlVaiTro, conn, tran))
                                {
                                    cmd.Parameters.AddWithValue("@VaiTro",  "VT_BGH");
                                    cmd.Parameters.AddWithValue("@TieuDe",  "📋 Đề xuất mua sắm chờ BGH phê duyệt");
                                    cmd.Parameters.AddWithValue("@NoiDung", "Phòng KHTC đã duyệt ngân sách đề xuất (Mã: " + id + "). Vui lòng phê duyệt cuối.");
                                    cmd.Parameters.AddWithValue("@Loai",    "pending");
                                    cmd.ExecuteNonQuery();
                                }
                                if (nguoiDX != null)
                                    using (var cmd = new SqlCommand(sqlUser, conn, tran))
                                    {
                                        cmd.Parameters.AddWithValue("@NguoiDung", nguoiDX);
                                        cmd.Parameters.AddWithValue("@TieuDe",    "✅ Đề xuất đã được Phòng KHTC duyệt ngân sách");
                                        cmd.Parameters.AddWithValue("@NoiDung",   "Đề xuất (Mã: " + id + ") đã qua KHTC, đang chờ BGH phê duyệt cuối.");
                                        cmd.Parameters.AddWithValue("@Loai",      "approved");
                                        cmd.ExecuteNonQuery();
                                    }
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                if (nguoiDX != null)
                                    using (var cmd = new SqlCommand(sqlUser, conn, tran))
                                    {
                                        cmd.Parameters.AddWithValue("@NguoiDung", nguoiDX);
                                        cmd.Parameters.AddWithValue("@TieuDe",    "❌ Đề xuất bị Phòng KHTC từ chối");
                                        cmd.Parameters.AddWithValue("@NoiDung",   "Đề xuất (Mã: " + id + ") bị KHTC từ chối. Lý do: " + ghiChu);
                                        cmd.Parameters.AddWithValue("@Loai",      "rejected");
                                        cmd.ExecuteNonQuery();
                                    }
                            }
                            catch { }
                        }
                        // Ghi lịch sử duyệt KHTC
                        try
                        {
                            using (var cmd = new SqlCommand(@"INSERT INTO LICHSUDUYET (ID_LichSu,DeXuatNo,CapDuyet,NguoiDuyetNo,ThoiGianDuyet,TrangThaiSauDuyet,GhiChu)
                                VALUES (LEFT(REPLACE(NEWID(),'-',''),10),@DX,3,@ND,GETDATE(),@TT,@GC)", conn, tran))
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
                    const string sql = @"SELECT ct.TenThietBiDeXuat, ct.SoLuong, ct.GiaDuKien, ct.DonViTinh
                        FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo=@Id";
                    var items = new System.Collections.Generic.List<object>();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                items.Add(new {
                                    Ten      = r["TenThietBiDeXuat"].ToString(),
                                    SoLuong  = r["SoLuong"],
                                    Gia      = r["GiaDuKien"],
                                    DVT      = r["DonViTinh"] == DBNull.Value ? "" : r["DonViTinh"].ToString(),
                                    DanhMuc  = ""
                                });
                    }
                    return Json(new { ok = true, data = items }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        [HttpGet]
        public JsonResult GetLichSuSuaChua(string id)
        {
            var history = new List<object>();
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    // Truy vấn xuyên qua bảng trung gian CHITIET_KEHOACH
                    string sql = @"
                SELECT gn.NgayThucHien, gn.KetQua, gn.ChiPhiThucTe 
                FROM GHINHAN_SUA_CHUA gn
                INNER JOIN CHITIET_KEHOACH ckh ON gn.ChiTietKeHoachNo = ckh.ID_ChiTietKH
                WHERE ckh.ThietBiNo = @id
                ORDER BY gn.NgayThucHien DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                history.Add(new
                                {
                                    Ngay = Convert.ToDateTime(rdr["NgayThucHien"]).ToString("dd/MM/yyyy"),
                                    NoiDung = rdr["KetQua"].ToString(),
                                    Tien = Convert.ToDecimal(rdr["ChiPhiThucTe"])
                                });
                            }
                        }
                    }
                }
                return Json(new { ok = true, data = history }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = "Lỗi kết nối: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
