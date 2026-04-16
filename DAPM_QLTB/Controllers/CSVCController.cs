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
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return null;
        }

        public ActionResult QuanLyThietBi()
        {
            var list = new List<ThietBiViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
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
        public ActionResult LapKeHoachBaoTri() { var r = CheckAuth(); return r ?? View(); }
        public ActionResult GhiNhanSuaChua() { var r = CheckAuth(); return r ?? View(); }
        public ActionResult KiemKeTaiSan() { var r = CheckAuth(); return r ?? View(); }

        // Ajax: lấy chi tiết thiết bị của 1 phiếu đề xuất
        [HttpGet]
        public JsonResult GetChiTiet(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"SELECT TenThietBiDeXuat, SoLuong, GiaDuKien, DonViTinh
                                         FROM CHITIET_DEXUAT WHERE DeXuatNo = @Id";
                    var list = new System.Collections.Generic.List<object>();
                    using (var cmd = new SqlCommand(sql, conn))
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

        // ===================== PHÊ DUYỆT ĐỀ XUẤT =====================
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
                        SELECT dx.ID_DeXuat, dx.NgayDeXuat, dx.TrangThai, dx.MoTa, dx.LyDoTuChoi,
                               nd.HoTen AS NguoiDeXuat,
                               ISNULL(kp.TenPhongBanKhoa, N'') AS KhoaPhongBan,
                               ISNULL((SELECT SUM(ct.SoLuong * ISNULL(ct.GiaDuKien,0))
                                       FROM CHITIET_DEXUAT ct WHERE ct.DeXuatNo = dx.ID_DeXuat), 0) AS TongGia
                        FROM   DEXUAT_MUASAM dx
                        JOIN   NGUOIDUNG nd ON nd.ID_NguoiDung = dx.NguoiDeXuatNo
                        LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = nd.Khoa_BanNo
                        ORDER  BY dx.NgayDeXuat DESC";

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
        [HttpGet]
        public JsonResult GetThietBiById(string id)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
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
        // ===================== THÊM & SỬA THIẾT BỊ (Dùng chung) =====================
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
                        // Thêm mới → tự sinh ID
                        tb.ID_ThietBi = GenerateNewThietBiID(conn);
                    }
                    else if (IdExists(conn, tb.ID_ThietBi) == false)
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
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM THIETBI WHERE ID_ThietBi = @id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return Json(new { ok = true });
        }

        // POST: CSVC duyệt hoặc từ chối → cập nhật trạng thái + gửi thông báo
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
                        // Cập nhật trạng thái
                        using (var cmd = new SqlCommand(
                            @"UPDATE DEXUAT_MUASAM
                              SET TrangThai    = @TrangThai,
                                  LyDoTuChoi   = CASE WHEN @Action='tuchoi' THEN @GhiChu ELSE LyDoTuChoi END,
                                  NgayDuyetCuoi = GETDATE()
                              WHERE ID_DeXuat = @Id", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@TrangThai", trangThaiMoi);
                            cmd.Parameters.AddWithValue("@Action", action ?? "");
                            cmd.Parameters.AddWithValue("@GhiChu", (object)ghiChu ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }

                        // Lấy người đề xuất
                        string nguoiDX = LayNguoiDeXuat(conn, tran, id);

                        if (action == "duyet")
                        {
                            try
                            {
                                // Báo KHTC có việc mới
                                GuiThongBaoVaiTro(conn, tran, "VT_KHTC",
                                    "📋 Đề xuất mua sắm chờ duyệt ngân sách",
                                    "Phòng CSVC đã xác nhận đề xuất (Mã: " + id + "). Vui lòng phê duyệt ngân sách.",
                                    "pending");
                                // Báo Trưởng Khoa biết CSVC đã duyệt
                                if (nguoiDX != null)
                                    GuiThongBaoNguoiDung(conn, tran, nguoiDX,
                                        "✅ Đề xuất đã được Phòng CSVC xác nhận",
                                        "Đề xuất (Mã: " + id + ") đã qua CSVC, đang chờ Phòng KHTC duyệt ngân sách.",
                                        "approved");
                            }
                            catch { /* bảng THONGBAO chưa tạo thì bỏ qua */ }
                        }
                        else
                        {
                            try
                            {
                                // Báo Trưởng Khoa bị từ chối
                                if (nguoiDX != null)
                                    GuiThongBaoNguoiDung(conn, tran, nguoiDX,
                                        "❌ Đề xuất bị Phòng CSVC từ chối",
                                        "Đề xuất (Mã: " + id + ") bị CSVC từ chối. Lý do: " + ghiChu,
                                        "rejected");
                            }
                            catch { }
                        }

                        tran.Commit();
                    }
                }
                return Json(new { ok = true });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // ===================== HELPER =====================
        private string LayNguoiDeXuat(SqlConnection conn, SqlTransaction tran, string idDX)
        {
            using (var cmd = new SqlCommand("SELECT NguoiDeXuatNo FROM DEXUAT_MUASAM WHERE ID_DeXuat=@Id", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Id", idDX);
                var v = cmd.ExecuteScalar();
                return v == null ? null : v.ToString();
            }
        }

        private void GuiThongBaoVaiTro(SqlConnection conn, SqlTransaction tran, string vaiTro, string tieuDe, string noiDung, string loai)
        {
            using (var cmd = new SqlCommand(
                @"INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
                  SELECT NEWID(), vn.NguoiDungNo, @TieuDe, @NoiDung, GETDATE(), @Loai, 0
                  FROM   VAITRO_NGUOIDUNG vn WHERE vn.VaiTroNo = @VaiTro", conn, tran))
            {
                cmd.Parameters.AddWithValue("@VaiTro", vaiTro);
                cmd.Parameters.AddWithValue("@TieuDe", tieuDe);
                cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                cmd.Parameters.AddWithValue("@Loai", loai);
                cmd.ExecuteNonQuery();
            }
        }

        private void GuiThongBaoNguoiDung(SqlConnection conn, SqlTransaction tran, string nguoiDungId, string tieuDe, string noiDung, string loai)
        {
            using (var cmd = new SqlCommand(
                @"INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
                  VALUES (NEWID(), @NguoiDung, @TieuDe, @NoiDung, GETDATE(), @Loai, 0)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@NguoiDung", nguoiDungId);
                cmd.Parameters.AddWithValue("@TieuDe", tieuDe);
                cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                cmd.Parameters.AddWithValue("@Loai", loai);
                cmd.ExecuteNonQuery();
            }
        }
        private string GenerateNewThietBiID(SqlConnection conn)
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
