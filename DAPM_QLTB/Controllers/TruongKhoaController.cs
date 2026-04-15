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

        [HttpGet]
        public ActionResult DeXuatMuaSam()
        {
            var dt = new DataTable();
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Kiểm tra cột DonViTinh có tồn tại không
                    bool hasDonViTinh = false;
                    using (var chk = new SqlCommand(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CHITIET_DEXUAT' AND COLUMN_NAME='DonViTinh'", conn))
                    {
                        hasDonViTinh = (int)chk.ExecuteScalar() > 0;
                    }

                    string colDVT = hasDonViTinh ? "c.DonViTinh," : "N'' AS DonViTinh,";
                    string sql = @"SELECT d.ID_DeXuat, d.NgayDeXuat, d.TrangThai, d.MoTa, d.LyDoTuChoi, d.NgayDuyetCuoi,
                                          c.TenThietBiDeXuat, c.SoLuong, c.GiaDuKien, " + colDVT + @"
                                          c.DeXuatNo
                                   FROM DEXUAT_MUASAM d
                                   JOIN CHITIET_DEXUAT c ON d.ID_DeXuat = c.DeXuatNo
                                   WHERE d.NguoiDeXuatNo = @UserId
                                   ORDER BY d.NgayDeXuat DESC";
                    var da = new SqlDataAdapter(sql, conn);
                    da.SelectCommand.Parameters.AddWithValue("@UserId", Session["UserId"]?.ToString() ?? "");
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi tải dữ liệu: " + ex.Message;
            }
            return View(dt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiDeXuat(string[] tenTB, int[] soluong, decimal[] gia, string[] donvi, string mota)
        {
            if (tenTB == null || tenTB.Length == 0)
            {
                ViewBag.Error = "Vui lòng nhập ít nhất 1 thiết bị.";
                return View("DeXuatMuaSam", LoadDeXuatCuaUser());
            }

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    string idDX = Guid.NewGuid().ToString().Substring(0, 10);
                    string user = Session["UserId"]?.ToString() ?? "";

                    // 1 phiếu đề xuất
                    var cmd1 = new SqlCommand(
                        "INSERT INTO DEXUAT_MUASAM (ID_DeXuat, NguoiDeXuatNo, NgayDeXuat, TrangThai, MoTa) VALUES (@id,@user,GETDATE(),N'Chờ CSVC duyệt',@mota)",
                        conn, tran);
                    cmd1.Parameters.AddWithValue("@id",   idDX);
                    cmd1.Parameters.AddWithValue("@user", user);
                    cmd1.Parameters.AddWithValue("@mota", (object)mota ?? DBNull.Value);
                    cmd1.ExecuteNonQuery();

                    // Nhiều dòng chi tiết
                    int soThietBi = 0;

                    // Kiểm tra cột DonViTinh
                    bool hasDVT = false;
                    using (var chk = new SqlCommand(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CHITIET_DEXUAT' AND COLUMN_NAME='DonViTinh'", conn, tran))
                    {
                        hasDVT = (int)chk.ExecuteScalar() > 0;
                    }

                    // Kiểm tra tên PK của CHITIET_DEXUAT
                    string pkCol = "ID_CTDeXuat"; // default
                    using (var chkPK = new SqlCommand(
                        @"SELECT TOP 1 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
                          WHERE TABLE_NAME='CHITIET_DEXUAT' AND ORDINAL_POSITION=1", conn, tran))
                    {
                        var val = chkPK.ExecuteScalar();
                        if (val != null) pkCol = val.ToString();
                    }

                    for (int i = 0; i < tenTB.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(tenTB[i])) continue;
                        string insertCT = hasDVT
                            ? "INSERT INTO CHITIET_DEXUAT (" + pkCol + ",DeXuatNo,TenThietBiDeXuat,SoLuong,GiaDuKien,DonViTinh) VALUES (@ct,@dx,@ten,@sl,@gia,@dvt)"
                            : "INSERT INTO CHITIET_DEXUAT (" + pkCol + ",DeXuatNo,TenThietBiDeXuat,SoLuong,GiaDuKien) VALUES (@ct,@dx,@ten,@sl,@gia)";
                        var cmd2 = new SqlCommand(insertCT, conn, tran);
                        cmd2.Parameters.AddWithValue("@ct",  Guid.NewGuid().ToString().Substring(0, 10));
                        cmd2.Parameters.AddWithValue("@dx",  idDX);
                        cmd2.Parameters.AddWithValue("@ten", tenTB[i]);
                        cmd2.Parameters.AddWithValue("@sl",  soluong != null && i < soluong.Length ? soluong[i] : 1);
                        cmd2.Parameters.AddWithValue("@gia", gia     != null && i < gia.Length     ? gia[i]     : 0m);
                        if (hasDVT)
                            cmd2.Parameters.AddWithValue("@dvt", donvi != null && i < donvi.Length && !string.IsNullOrEmpty(donvi[i]) ? (object)donvi[i] : DBNull.Value);
                        cmd2.ExecuteNonQuery();
                        soThietBi++;
                    }

                    // Thông báo cho CSVC
                    var cmdTB = new SqlCommand(
                        @"INSERT INTO THONGBAO (ID_ThongBao,NguoiNhanNo,TieuDe,NoiDung,NgayTao,LoaiThongBao,DaDoc)
                          SELECT NEWID(),vn.NguoiDungNo,@TieuDe,@NoiDung,GETDATE(),N'pending',0
                          FROM VAITRO_NGUOIDUNG vn WHERE vn.VaiTroNo=N'VT_CSVC'",
                        conn, tran);
                    cmdTB.Parameters.AddWithValue("@TieuDe",  "📋 Đề xuất mua sắm mới cần xét duyệt");
                    cmdTB.Parameters.AddWithValue("@NoiDung", "Trưởng khoa đã gửi đề xuất " + soThietBi + " thiết bị (Mã: " + idDX + "). Vui lòng xem xét và phê duyệt.");
                    cmdTB.ExecuteNonQuery();

                    tran.Commit();
                    TempData["Success"] = "Gửi đề xuất thành công! " + soThietBi + " thiết bị đang chờ Phòng CSVC xét duyệt.";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ViewBag.Error = "Lỗi: " + ex.Message;
                    return View("DeXuatMuaSam", LoadDeXuatCuaUser());
                }
            }
            return RedirectToAction("DeXuatMuaSam");
        }

        private DataTable LoadDeXuatCuaUser()
        {
            var dt = new DataTable();
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT d.ID_DeXuat, d.NgayDeXuat, d.TrangThai, d.MoTa, d.LyDoTuChoi, d.NgayDuyetCuoi,
                                          c.TenThietBiDeXuat, c.SoLuong, c.GiaDuKien, c.DonViTinh
                                   FROM DEXUAT_MUASAM d
                                   JOIN CHITIET_DEXUAT c ON d.ID_DeXuat = c.DeXuatNo
                                   WHERE d.NguoiDeXuatNo = @UserId
                                   ORDER BY d.NgayDeXuat DESC";
                    var da = new SqlDataAdapter(sql, conn);
                    da.SelectCommand.Parameters.AddWithValue("@UserId", Session["UserId"]?.ToString() ?? "");
                    da.Fill(dt);
                }
            }
            catch { }
            return dt;
        }

        public ActionResult GuiDeXuat()  => RedirectToAction("DeXuatMuaSam");
        public ActionResult XemDeXuat()  => RedirectToAction("DeXuatMuaSam");

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
                    var cmd = new SqlCommand(
                        "INSERT INTO BAOHONG_THIETBI (ID_BaoHong,ThietBiNo,NguoiBaoHongNo,MoTaHong,NgayBao,MucDo,TrangThai) VALUES (@id,@tb,@user,@mota,GETDATE(),N'Cao',N'Chờ xử lý')",
                        conn);
                    cmd.Parameters.AddWithValue("@id",   Guid.NewGuid().ToString().Substring(0, 10));
                    cmd.Parameters.AddWithValue("@tb",   id);
                    cmd.Parameters.AddWithValue("@user", Session["UserId"]?.ToString() ?? "");
                    cmd.Parameters.AddWithValue("@mota", mota ?? "");
                    cmd.ExecuteNonQuery();
                }
                TempData["Success"] = "Đã gửi báo cáo hỏng!";
            }
            catch { }
            return RedirectToAction("DanhSachThietBi");
        }
    }
}
