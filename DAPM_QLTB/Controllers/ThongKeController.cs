using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly ThongKeService _svc = new ThongKeService();

        private ActionResult RequireRole(int role)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != role)
                return RedirectToAction("Index", "Home");
            return null;
        }

        // ── BGH ──────────────────────────────────────────────
        public ActionResult Dashboard()
        {
            var r = RequireRole(4); if (r != null) return r;
            try { return View(_svc.GetBGHDashboard()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new BGHDashboardViewModel()); }
        }

        public ActionResult ThongKeTaiSan()
        {
            var r = RequireRole(4); if (r != null) return r;
            try { return View(_svc.GetThongKeTaiSan()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new ThongKeTaiSanViewModel()); }
        }

        public ActionResult TheoDoi()
        {
            var r = RequireRole(4); if (r != null) return r;
            try { return View(_svc.GetTheoDoi()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new System.Collections.Generic.List<TheDoiThietBiViewModel>()); }
        }

        // ── KHTC ─────────────────────────────────────────────
        public ActionResult DashboardKHTC()
        {
            var r = RequireRole(3); if (r != null) return r;
            try { return View(_svc.GetKHTCDashboard()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new KHTCDashboardViewModel()); }
        }

        public ActionResult BaoCaoTaiSan()
        {
            var r = RequireRole(3); if (r != null) return r;
            try { return View(_svc.GetBaoCaoTaiSan()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new System.Collections.Generic.List<BaoCaoTaiSanViewModel>()); }
        }
    }
}
