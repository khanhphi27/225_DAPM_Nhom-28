using System;
using System.Web.Mvc;

namespace QLTB.Controllers
{
    public class CSVCController : Controller
    {
        private void CheckAuth()
        {
        }

        // GET: CSVC/QuanLyThietBi
        public ActionResult QuanLyThietBi()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: CSVC/PheDuyetDeXuat
        public ActionResult PheDuyetDeXuat()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: CSVC/LapKeHoachBaoTri
        public ActionResult LapKeHoachBaoTri()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: CSVC/GhiNhanSuaChua
        public ActionResult GhiNhanSuaChua()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: CSVC/KiemKeTaiSan
        public ActionResult KiemKeTaiSan()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            return View();
        }
    }
}
