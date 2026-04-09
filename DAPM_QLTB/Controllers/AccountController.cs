using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;
using QLTB.Models;

namespace QLTB.Controllers
{
    public class AccountController : Controller
    {
        // Map VaiTro ID -> RoleId số (dùng trong toàn bộ hệ thống)
        // VT_TK=1, VT_CSVC=2, VT_KHTC=3, VT_BGH=4
        private int MapVaiTroToRoleId(string vaiTroId)
        {
            switch (vaiTroId)
            {
                case "VT_TK":   return 1;
                case "VT_CSVC": return 2;
                case "VT_KHTC": return 3;
                case "VT_BGH":  return 4;
                default:        return 0;
            }
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    // Query khớp đúng với cấu trúc DB: NGUOIDUNG + VAITRO_NGUOIDUNG
                    const string sql = @"
                        SELECT u.ID_NguoiDung, u.HoTen, u.Email,
                               vn.VaiTroNo
                        FROM   NGUOIDUNG u
                        LEFT JOIN VAITRO_NGUOIDUNG vn ON vn.NguoiDungNo = u.ID_NguoiDung
                        WHERE  u.ID_NguoiDung = @Username
                          AND  u.MatKhau      = @Password
                          AND  u.TrangThaiTK  = 1";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", model.Username);
                        cmd.Parameters.AddWithValue("@Password", model.Password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string userId   = reader["ID_NguoiDung"].ToString();
                                string hoTen    = reader["HoTen"].ToString();
                                string email    = reader["Email"].ToString();
                                string vaiTroId = reader["VaiTroNo"] == DBNull.Value
                                                  ? "" : reader["VaiTroNo"].ToString().Trim();
                                int roleId = MapVaiTroToRoleId(vaiTroId);

                                FormsAuthentication.SetAuthCookie(userId, model.RememberMe);
                                Session["UserId"]    = userId;
                                Session["UserName"]  = hoTen;
                                Session["UserRole"]  = roleId;
                                Session["UserEmail"] = email;

                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi kết nối CSDL: " + ex.Message);
                return View(model);
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/Profile
        public ActionResult Profile()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: Account/Notifications
        public ActionResult Notifications()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return View();
        }
    }
}
