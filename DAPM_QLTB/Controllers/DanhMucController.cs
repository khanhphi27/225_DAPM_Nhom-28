using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly DanhMucService _svc = new DanhMucService();

        private ActionResult CheckAuth()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return null;
        }

        public ActionResult QuanLy()
        {
            var r = CheckAuth(); if (r != null) return r;
            try { return View(_svc.GetAll()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new System.Collections.Generic.List<DanhMucViewModel>()); }
        }

        [HttpGet]
        public JsonResult GetDanhMuc(string id)
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
        public JsonResult SaveDanhMuc(string id, string ten, string moTa)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Save(id, ten, moTa); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public JsonResult XoaDanhMuc(string id)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Delete(id); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }
    }
}
