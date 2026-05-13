using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class KhoaController : Controller
    {
        private readonly KhoaService _svc = new KhoaService();

        private ActionResult RequireRole(int role)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != role)
                return RedirectToAction("Index", "Home");
            return null;
        }

        public ActionResult QuanLy()
        {
            var r = RequireRole(4); if (r != null) return r;
            try { return View(_svc.GetAll()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new System.Collections.Generic.List<KhoaPhongBanViewModel>()); }
        }

        [HttpGet]
        public JsonResult GetKhoa(string id)
        {
            try
            {
                var data = _svc.GetById(id);
                return data != null
                    ? Json(new { ok = true, data }, JsonRequestBehavior.AllowGet)
                    : Json(new { ok = false, msg = "Không tìm thấy." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpPost]
        public JsonResult TaoKhoa(string id, string ten)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Create(id, ten); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public JsonResult SuaKhoa(string id, string ten)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Update(id, ten); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public JsonResult XoaKhoa(string id)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Delete(id); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }
    }
}
