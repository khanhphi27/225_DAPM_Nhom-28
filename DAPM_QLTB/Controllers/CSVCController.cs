using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using QLTB.Models;

namespace QLTB.Controllers
{
    public class CSVCController : Controller
    {
        private ActionResult CheckAuth()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return null;
        }

        public ActionResult QuanLyThietBi()
        {
            var r = CheckAuth(); if (r != null) return r;
            return View();
        }

        // GET: CSVC/PheDuyetDeXuat
        public ActionResult PheDuyetDeXuat()
        {
            var r = CheckAuth(); if (r != null) return r;

            var choDuyet = new List<DeXuatViewModel>();
            var lichSu   = new List<DeXuatViewModel>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT dx.ID_DeXuat, dx.NgayDeXuat, dx.TrangThai,
                               dx.MoTa, dx.LyDoTuChoi,
                               nd.HoTen AS NguoiDeXuat, kp.TenPhongBanKhoa AS KhoaPhongBan,
                               ISNULL((SELECT SUM(ct.SoLuong * ISNULL(ct.GiaDuKien,0))
                                       FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo = dx.ID_DeXuat), 0) AS TongGia
                        FROM DEXUAT_MUASAM dx
                        JOIN NGUOIDUNG nd ON nd.ID_NguoiDung = dx.NguoiDeXuatNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = nd.Khoa_BanNo
                        ORDER BY dx.NgayDeXuat DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            var item = new DeXuatViewModel
                            {
                                ID_DeXuat     = reader["ID_DeXuat"].ToString(),
                                NguoiDeXuat   = reader["NguoiDeXuat"].ToString(),
                                KhoaPhongBan  = reader["KhoaPhongBan"].ToString(),
                                NgayDeXuat    = Convert.ToDateTime(reader["NgayDeXuat"]),
                                TrangThai     = reader["TrangThai"].ToString(),
                                MoTa          = reader["MoTa"] == DBNull.Value ? "" : reader["MoTa"].ToString(),
                                LyDoTuChoi    = reader["LyDoTuChoi"] == DBNull.Value ? "" : reader["LyDoTuChoi"].ToString(),
                                TongGiaDuKien = Convert.ToDecimal(reader["TongGia"])
                            };
                            if (item.TrangThai == "Chờ CSVC duyệt")
                                choDuyet.Add(item);
                            else
                                lichSu.Add(item);
                        }
                }
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }

            ViewBag.ChoDuyet = choDuyet;
            ViewBag.LichSu   = lichSu;
            return View();
        }

        // POST: CSVC/XuLyDeXuat
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
                        const string sqlUpdate = @"
                            UPDATE DEXUAT_MUASAM
                            SET TrangThai  = @TrangThai,
                                LyDoTuChoi = CASE WHEN @Action = 'tuchoi' THEN @GhiChu ELSE LyDoTuChoi END,
                                NgayDuyetCuoi = GETDATE()
                            WHERE ID_DeXuat = @Id";
                        using (var cmd = new SqlCommand(sqlUpdate, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@TrangThai", trangThaiMoi);
                            cmd.Parameters.AddWithValue("@Action",    action ?? "");
                            cmd.Parameters.AddWithValue("@GhiChu",    (object)ghiChu ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Id",        id);
                            cmd.ExecuteNonQuery();
                        }

                        // Lấy người đề xuất
                        string nguoiDeXuatId = null;
                        using (var cmd = new SqlCommand("SELECT NguoiDeXuatNo FROM DEXUAT_MUASAM WHERE ID_DeXuat=@Id", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            var val = cmd.ExecuteScalar();
                            if (val != null) nguoiDeXuatId = val.ToString();
                        }

                        if (action == "duyet")
                        {
                            // Thông báo KHTC
                            GuiThongBaoVaiTro(conn, tran, "VT_KHTC",
                                "📋 Đề xuất mua sắm chờ duyệt ngân sách",
                                "Phòng CSVC đã xác nhận đề xuất (Mã: " + id + "). Vui lòng phê duyệt ngân sách.",
                                "pending");
                            // Thông báo Trưởng Khoa
                            if (nguoiDeXuatId != null)
                                GuiThongBaoNguoiDung(conn, tran, nguoiDeXuatId,
                                    "✅ Đề xuất đã được Phòng CSVC xác nhận",
                                    "Đề xuất (Mã: " + id + ") đã được CSVC xác nhận, chuyển lên KHTC duyệt ngân sách.",
                                    "approved");
                        }
                        else
                        {
                            if (nguoiDeXuatId != null)
                                GuiThongBaoNguoiDung(conn, tran, nguoiDeXuatId,
                                    "❌ Đề xuất bị Phòng CSVC từ chối",
                                    "Đề xuất (Mã: " + id + ") bị CSVC từ chối. Lý do: " + ghiChu,
                                    "rejected");
                        }

                        tran.Commit();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        private void GuiThongBaoVaiTro(SqlConnection conn, SqlTransaction tran, string vaiTro, string tieuDe, string noiDung, string loai)
        {
            const string sql = @"INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
                SELECT NEWID(), vn.NguoiDungNo, @TieuDe, @NoiDung, GETDATE(), @Loai, 0
                FROM VAITRO_NGUOIDUNG vn WHERE vn.VaiTroNo = @VaiTro";
            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@VaiTro",  vaiTro);
                cmd.Parameters.AddWithValue("@TieuDe",  tieuDe);
                cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                cmd.Parameters.AddWithValue("@Loai",    loai);
                cmd.ExecuteNonQuery();
            }
        }

        private void GuiThongBaoNguoiDung(SqlConnection conn, SqlTransaction tran, string nguoiDungId, string tieuDe, string noiDung, string loai)
        {
            const string sql = @"INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
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

        public ActionResult LapKeHoachBaoTri()
        {
            var r = CheckAuth(); if (r != null) return r;
            return View();
        }

        public ActionResult GhiNhanSuaChua()
        {
            var r = CheckAuth(); if (r != null) return r;
            return View();
        }

        public ActionResult KiemKeTaiSan()
        {
            var r = CheckAuth(); if (r != null) return r;
            return View();
        }
    }
}
