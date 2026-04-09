using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLTB.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Kiểm tra xem user đã đăng nhập chưa
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = Session["UserName"];
            ViewBag.UserRole = GetRoleName((int)Session["UserRole"]);
            ViewBag.UserEmail = Session["UserEmail"];

            return View();
        }

        private string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    return "Trưởng Khoa";
                case 2:
                    return "Phòng CSVC";
                case 3:
                    return "Phòng KHTC";
                case 4:
                    return "BGH";
                default:
                    return "Người dùng";
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}