using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Mvc;

namespace QLTB.Controllers
{
    public class TruongKhoaController : Controller
    {
        private string connStr = ConfigurationManager.ConnectionStrings["QuanLyThietBi"].ConnectionString;

        public ActionResult Index() => View();

        // ===================== ĐỀ XUẤT MUA SẮM =====================
        [HttpGet]
        public ActionResult DeXuatMuaSam()
        {
            var dt = new DataTable();
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT d.ID_DeXuat, d.NgayDeXuat, d.TrangThai, d.MoTa, d.LyDoTuChoi, d.NgayDuyetCuoi,
                               c.TenThietBiDeXuat, c.SoLuong, c.GiaDuKien, c.DonViTinh
                        FROM   DEXUAT_MUASAM d
                        JOIN   CHITIET_DEXUAT c ON c.DeXuatNo = d.ID_DeXuat
                        WHERE  d.NguoiDeXuatNo = @UserId
                        ORDER  BY d.NgayDeXuat DESC";
                    var da = new SqlDataAdapter(sql, conn);
                    da.SelectCommand.Parameters.AddWithValue("@UserId", Session["UserId"]?.ToString() ?? "");
                    da.Fill(dt);
                }
            }
            catch (Exception ex) { ViewBag.Error = "Lỗi tải dữ liệu: " + ex.Message; }
            return View(dt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiDeXuat(string[] tenTB, int[] soluong, decimal[] gia, string[] donvi, string mota)
        {
            if (tenTB == null || tenTB.Length == 0)
            {
                ViewBag.Error = "Vui lòng nhập ít nhất 1 thiết bị.";
                return RedirectToAction("DeXuatMuaSam");
            }

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    // Tạo ID 10 ký tự từ NEWID() - đảm bảo vừa CHAR(10)
                    string idDX;
                    using (var cmdId = new SqlCommand("SELECT LEFT(REPLACE(NEWID(),'-',''),10)", conn, tran))
                    {
                        idDX = cmdId.ExecuteScalar().ToString();
                    }
                    string user = Session["UserId"]?.ToString() ?? "";

                    // 1. Tạo phiếu đề xuất — trạng thái ban đầu: Chờ CSVC duyệt
                    using (var cmd = new SqlCommand(
                        @"INSERT INTO DEXUAT_MUASAM (ID_DeXuat, NguoiDeXuatNo, NgayDeXuat, TrangThai, MoTa)
                          VALUES (@id, @user, GETDATE(), N'Chờ CSVC duyệt', @mota)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id",   idDX);
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.Parameters.AddWithValue("@mota", (object)mota ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Insert từng dòng chi tiết thiết bị
                    int dem = 0;
                    for (int i = 0; i < tenTB.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(tenTB[i])) continue;
                        using (var cmd = new SqlCommand(
                            @"INSERT INTO CHITIET_DEXUAT (ID_ChiTiet, DeXuatNo, TenThietBiDeXuat, SoLuong, GiaDuKien, DonViTinh)
                              VALUES (LEFT(REPLACE(NEWID(),'-',''),10), @dx, @ten, @sl, @gia, @dvt)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@dx",  idDX);
                            cmd.Parameters.AddWithValue("@ten", tenTB[i]);
                            cmd.Parameters.AddWithValue("@sl",  soluong != null && i < soluong.Length ? soluong[i] : 1);
                            cmd.Parameters.AddWithValue("@gia", gia     != null && i < gia.Length     ? gia[i]     : 0m);
                            cmd.Parameters.AddWithValue("@dvt", donvi   != null && i < donvi.Length && !string.IsNullOrEmpty(donvi[i])
                                                                    ? (object)donvi[i] : DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        dem++;
                    }

                    tran.Commit();
                    TempData["Success"] = "Gửi đề xuất thành công! " + dem + " thiết bị đang chờ Phòng CSVC xét duyệt.";

                    // Gửi thông báo sau khi commit (không ảnh hưởng đến đề xuất nếu lỗi)
                    try
                    {
                        using (var conn2 = new SqlConnection(connStr))
                        {
                            conn2.Open();
                            using (var cmdTB = new SqlCommand(
                                @"INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
                                  SELECT NEWID(), vn.NguoiDungNo, @TieuDe, @NoiDung, GETDATE(), N'pending', 0
                                  FROM VAITRO_NGUOIDUNG vn WHERE vn.VaiTroNo = N'VT_CSVC'", conn2))
                            {
                                cmdTB.Parameters.AddWithValue("@TieuDe",  "📋 Đề xuất mua sắm mới cần xét duyệt");
                                cmdTB.Parameters.AddWithValue("@NoiDung", "Trưởng khoa đã gửi đề xuất " + dem + " thiết bị (Mã: " + idDX + "). Vui lòng xem xét và phê duyệt.");
                                cmdTB.ExecuteNonQuery();
                            }
                        }
                    }
                    catch { /* Thông báo lỗi không ảnh hưởng đề xuất */ }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    TempData["Error"] = "Lỗi: " + ex.Message;
                }
            }
            return RedirectToAction("DeXuatMuaSam");
        }

        // ===================== DANH SÁCH THIẾT BỊ =====================
        public ActionResult DanhSachThietBi()
        {
            var dt = new DataTable();
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    new SqlDataAdapter("SELECT ID_ThietBi, TenTB, Gia, ThongSoKT, TrangThaiTB FROM THIETBI", conn).Fill(dt);
                }
            }
            catch (Exception ex) { ViewBag.Error = "Lỗi: " + ex.Message; }
            return View(dt);
        }

        [HttpPost]
        public ActionResult BaoHong(string id, string mota)
        {
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        @"INSERT INTO BAOHONG_THIETBI (ID_BaoHong, ThietBiNo, NguoiBaoHongNo, MoTaHong, NgayBao, MucDo, TrangThai)
                          VALUES (@id, @tb, @user, @mota, GETDATE(), N'Cao', N'Chờ xử lý')", conn))
                    {
                        cmd.Parameters.AddWithValue("@id",   "BH_" + DateTime.Now.ToString("yyMMddHHmmss"));
                        cmd.Parameters.AddWithValue("@tb",   id);
                        cmd.Parameters.AddWithValue("@user", Session["UserId"]?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@mota", mota ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["Success"] = "Đã gửi báo cáo hỏng!";
            }
            catch { }
            return RedirectToAction("DanhSachThietBi");
        }

        // Redirect link cũ
        public ActionResult GuiDeXuat() => RedirectToAction("DeXuatMuaSam");
        public ActionResult XemDeXuat() => RedirectToAction("DeXuatMuaSam");

        // ===================== HELPER =====================
        private void GuiThongBaoVaiTro(SqlConnection conn, SqlTransaction tran, string vaiTro, string tieuDe, string noiDung, string loai)
        {
            using (var cmd = new SqlCommand(
                @"INSERT INTO THONGBAO (ID_ThongBao, NguoiNhanNo, TieuDe, NoiDung, NgayTao, LoaiThongBao, DaDoc)
                  SELECT NEWID(), vn.NguoiDungNo, @TieuDe, @NoiDung, GETDATE(), @Loai, 0
                  FROM   VAITRO_NGUOIDUNG vn WHERE vn.VaiTroNo = @VaiTro", conn, tran))
            {
                cmd.Parameters.AddWithValue("@VaiTro",  vaiTro);
                cmd.Parameters.AddWithValue("@TieuDe",  tieuDe);
                cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                cmd.Parameters.AddWithValue("@Loai",    loai);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
