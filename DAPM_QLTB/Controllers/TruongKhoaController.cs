using System.Web.Mvc;

namespace QLTB.Controllers
{
    public class TruongKhoaController : Controller
    {
        private ActionResult RequireRole(int requiredRole)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != requiredRole)
                return RedirectToAction("Index", "Home");
            return null;
        }

        public ActionResult Index()
        {
            var redirect = RequireRole(1);
            if (redirect != null) return redirect;
            return View();
        }

        public ActionResult GuiDeXuat()
        {
            var redirect = RequireRole(1);
            if (redirect != null) return redirect;
            return View();
        }

        public ActionResult BaoHong()
        {
            var redirect = RequireRole(1);
            if (redirect != null) return redirect;
            return View();
        }

        public ActionResult XemDeXuat()
        {
            var redirect = RequireRole(1);
            if (redirect != null) return redirect;
            return View();
        }
    }
}
