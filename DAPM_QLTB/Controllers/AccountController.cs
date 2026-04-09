using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;
using QLTB.Models;

namespace QLTB.Controllers
{
    public class AccountController : Controller
    {
        // VT_TK=1 (Trưởng Khoa), VT_CSVC=2, VT_KHTC=3, VT_BGH=4
        private int MapVaiTroToRoleId(string vaiTroId)
        {
            if (string.IsNullOrEmpty(vaiTroId)) return 0;
            switch (vaiTroId.Trim())
            {
                case "VT_TK": return 1;
                case "VT_CSVC": return 2;
                case "VT_KHTC": return 3;
                case "VT_BGH": return 4;
                default: return 0;
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserId"] != null) return RedirectToDashboard((int)Session["UserRole"]);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    const string sql = @"
                        SELECT u.ID_NguoiDung, u.HoTen, u.Email, vn.VaiTroNo
                        FROM NGUOIDUNG u
                        LEFT JOIN VAITRO_NGUOIDUNG vn ON vn.NguoiDungNo = u.ID_NguoiDung
                        WHERE u.ID_NguoiDung = @Username AND u.MatKhau = @Password AND u.TrangThaiTK = 1";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", model.Username);
                        cmd.Parameters.AddWithValue("@Password", model.Password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string userId = reader["ID_NguoiDung"].ToString();
                                int roleId = MapVaiTroToRoleId(reader["VaiTroNo"]?.ToString());

                                // Lưu Session
                                FormsAuthentication.SetAuthCookie(userId, model.RememberMe);
                                Session["UserId"] = userId;
                                Session["UserName"] = reader["HoTen"].ToString();
                                Session["UserRole"] = roleId;

                                // ĐIỀU HƯỚNG DỰA TRÊN ROLE
                                return RedirectToDashboard(roleId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                return View(model);
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            return View(model);
        }

        // Hàm phụ trách điều hướng về đúng trang của từng chức vụ
        private ActionResult RedirectToDashboard(int roleId)
        {
            switch (roleId)
            {
                case 1: return RedirectToAction("Index", "TruongKhoa"); // Về Trưởng Khoa
                case 4: return RedirectToAction("Index", "BGH");        // Về Ban Giám Hiệu (nếu có)
                default: return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}