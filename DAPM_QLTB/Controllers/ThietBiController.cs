using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class ThietBiController : Controller
    {
        private readonly ThietBiService _svc = new ThietBiService();

        private ActionResult CheckAuth()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return null;
        }

        public ActionResult QuanLy(string deXuatNo = null)
        {
            var r = CheckAuth(); if (r != null) return r;
            if (!string.IsNullOrWhiteSpace(deXuatNo)) ViewBag.ImportDeXuatNo = deXuatNo.Trim();

            var data = _svc.GetAllForManagement();
            ViewBag.DanhMucList = data.danhMuc;
            ViewBag.KhoaList = data.khoa;
            ViewBag.CoSoList = data.coSo;
            ViewBag.KhuList = data.khu;
            ViewBag.NhaCungCapList = data.ncc;
            ViewBag.PhongList = data.phong;
            return View(data.list);
        }

        [HttpGet]
        public ActionResult ChiTiet(string id)
        {
            var r = CheckAuth(); if (r != null) return r;
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction("QuanLy");

            var detail = _svc.GetDetail(id);
            if (detail == null) return HttpNotFound();
            return View(detail);
        }

        [HttpGet]
        public JsonResult GetThietBiById(string id)
        {
            try
            {
                var data = _svc.GetById(id);
                return data != null
                    ? Json(new { ok = true, data }, JsonRequestBehavior.AllowGet)
                    : Json(new { ok = false, msg = "Không tìm thấy thiết bị" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpPost]
        public JsonResult SaveThietBi(ThietBiViewModel tb)
        {
            try { var (ok, msg) = _svc.Save(tb); return Json(new { ok, message = msg, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpGet]
        public JsonResult GetNextThietBiId(string nhaCCNo, string khoaNo, string danhMucNo)
        {
            var r = CheckAuth();
            if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." }, JsonRequestBehavior.AllowGet);
            try
            {
                var id = _svc.GenerateNextId(nhaCCNo, khoaNo, danhMucNo);
                return Json(new { ok = true, id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpPost]
        public JsonResult XoaThietBi(string id)
        {
            try { var (ok, msg) = _svc.Delete(id); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }
    }
}
