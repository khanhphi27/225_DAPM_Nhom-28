using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class DeXuatController : Controller
    {
        private readonly DeXuatService _svc = new DeXuatService();

        private ActionResult CheckAuth()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return null;
        }
        private string CurrentUser => Session["UserId"]?.ToString() ?? "";

        // ── CSVC: Phê duyệt đề xuất ─────────────────────────
        public ActionResult PheDuyet()
        {
            var r = CheckAuth(); if (r != null) return r;
            try
            {
                var (choDuyet, lichSu) = _svc.GetDeXuatForCSVC();
                ViewBag.ChoDuyet = choDuyet;
                ViewBag.LichSu = lichSu;
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; }
            return View();
        }

        [HttpPost]
        public ActionResult XuLyDeXuat(string id, string action, string ghiChu)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.XuLyDeXuat(id, action, ghiChu, CurrentUser); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpGet]
        public JsonResult GetChiTiet(string id)
        {
            try
            {
                var list = _svc.GetChiTiet(id);
                return Json(new { ok = true, data = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpPost]
        public ActionResult HoanTatNhapThietBi(string id)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.HoanTatNhapThietBi(id, CurrentUser); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // ── BGH: Lịch sử duyệt ──────────────────────────────
        [HttpGet]
        public JsonResult GetLichSuDuyet(string idDX)
        {
            try { return Json(new { ok = true, data = _svc.GetLichSuDuyet(idDX) }, JsonRequestBehavior.AllowGet); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpGet]
        public JsonResult GetDanhSachDeXuat()
        {
            try { return Json(new { ok = true, data = _svc.GetDanhSachDeXuat() }, JsonRequestBehavior.AllowGet); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
    }
}
