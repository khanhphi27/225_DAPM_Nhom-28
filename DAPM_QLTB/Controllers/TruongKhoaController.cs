using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Mvc;

namespace QLTB.Controllers
{
    public class TruongKhoaController : Controller
    {
        private string connStr = ConfigurationManager.ConnectionStrings["QuanLyThietBi"].ConnectionString;

        // Dashboard
        public ActionResult Index()
        {
            return View();
        }

        // G?I ?? XU?T - GET
        [HttpGet]
        public ActionResult GuiDeXuat() => View();

        // G?I ?? XU?T - POST (S? d?ng Transaction)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiDeXuat(string tenTB, int soluong, decimal gia, string mota)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string idDX = Guid.NewGuid().ToString().Substring(0, 10);
                    string idCT = Guid.NewGuid().ToString().Substring(0, 10);
                    string user = Session["UserId"]?.ToString() ?? "TK001";

                    // 1. Insert vào b?ng DEXUAT_MUASAM
                    string sql1 = @"INSERT INTO DEXUAT_MUASAM (ID_DeXuat, NguoiDeXuat, NgayDeXuat, TrangThai, TrangThaiXoa, GhiChu) 
                                    VALUES (@id, @user, GETDATE(), N'Ch? duy?t', 1, @mota)";
                    SqlCommand cmd1 = new SqlCommand(sql1, conn, transaction);
                    cmd1.Parameters.AddWithValue("@id", idDX);
                    cmd1.Parameters.AddWithValue("@user", user);
                    cmd1.Parameters.AddWithValue("@mota", mota ?? (object)DBNull.Value);
                    cmd1.ExecuteNonQuery();

                    // 2. Insert vào b?ng CHITIET_DEXUAT
                    string sql2 = @"INSERT INTO CHITIET_DEXUAT (ID_CTDeXuat, DeXuatNo, TenThietBiDeXuat, SoLuong, GiaDeXuat) 
                                    VALUES (@ct, @dx, @ten, @sl, @gia)";
                    SqlCommand cmd2 = new SqlCommand(sql2, conn, transaction);
                    cmd2.Parameters.AddWithValue("@ct", idCT);
                    cmd2.Parameters.AddWithValue("@dx", idDX);
                    cmd2.Parameters.AddWithValue("@ten", tenTB);
                    cmd2.Parameters.AddWithValue("@sl", soluong);
                    cmd2.Parameters.AddWithValue("@gia", gia);
                    cmd2.ExecuteNonQuery();

                    transaction.Commit();
                    TempData["Success"] = "G?i ?? xu?t thành công!";
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ViewBag.Error = "?ã x?y ra l?i khi g?i ?? xu?t.";
                    return View();
                }
            }
            return RedirectToAction("XemDeXuat");
        }
        // ================= XEM ?? XU?T =================
        public ActionResult XemDeXuat()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                // S?a l?i tên c?t cho ?úng v?i file SQL:
                // ID_DeXuat, NgayDeXuat, TrangThai thu?c b?ng DEXUAT_MUASAM
                // TenThietBiDeXuat, SoLuong, GiaDuKien thu?c b?ng CHITIET_DEXUAT
                string sql = @"SELECT 
                        d.NgayDeXuat, 
                        c.TenThietBiDeXuat, 
                        c.SoLuong, 
                        c.GiaDuKien, 
                        d.TrangThai
                       FROM DEXUAT_MUASAM d
                       JOIN CHITIET_DEXUAT c ON d.ID_DeXuat = c.DeXuatNo
                       ORDER BY d.NgayDeXuat DESC";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(dt);
            }
            return View(dt);
        }

        // ================= DANH SÁCH THI?T B? =================
        public ActionResult DanhSachThietBi()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    // L?y ?úng các c?t c?n thi?t, tránh dùng * n?u không ch?c ch?n
                    string sql = "SELECT ID_ThietBi, TenTB, Gia, ThongSoKT, TrangThaiTB FROM THIETBI";
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "L?i truy v?n: " + ex.Message;
            }
            return View(dt);
        }

        // BÁO H?NG
        [HttpPost]
        public ActionResult BaoHong(string id, string mota)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"INSERT INTO BAOHONG_THIETBI (ID_BaoHong, ThietBiID, NguoiBao, MoTa, NgayBao, MucDo, TrangThai) 
                                    VALUES (@idbh, @tb, @user, @mota, GETDATE(), N'Cao', N'Ch? x? lý')";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@idbh", Guid.NewGuid().ToString().Substring(0, 10));
                    cmd.Parameters.AddWithValue("@tb", id);
                    cmd.Parameters.AddWithValue("@user", Session["UserId"]?.ToString() ?? "TK001");
                    cmd.Parameters.AddWithValue("@mota", mota);
                    cmd.ExecuteNonQuery();
                }
                TempData["Success"] = "?ã g?i báo cáo h?ng!";
            }
            catch { /* Log error */ }
            return RedirectToAction("DanhSachThietBi");
        }
    }
}