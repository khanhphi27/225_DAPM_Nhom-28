using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class KiemKeController : Controller
    {
        private readonly KiemKeService _svc = new KiemKeService();

        private ActionResult CheckAuth()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return null;
        }

        public ActionResult DanhSach()
        {
            var r = CheckAuth(); if (r != null) return r;
            try { return View(_svc.GetKiemKeTaiSan()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new KiemKeTaiSanViewModel()); }
        }

        public ActionResult TaoPhieu()
        {
            var r = CheckAuth(); if (r != null) return r;
            try { return View(_svc.GetTaoPhieuData(Session["UserId"]?.ToString())); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new TaoPhieuKiemKeViewModel()); }
        }

        [HttpPost]
        public ActionResult HoanTat(TaoPhieuKiemKeViewModel model)
        {
            var r = CheckAuth(); if (r != null) return RedirectToAction("Login", "Account");
            try
            {
                var (ok, msg) = _svc.HoanTatKiemKe(model);
                TempData[ok ? "Success" : "Error"] = msg;
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("DanhSach");
        }
    }
}
