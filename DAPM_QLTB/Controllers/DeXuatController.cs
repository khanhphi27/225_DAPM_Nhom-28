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

        // ── BGH: Xét duyệt yêu cầu ──────────────────────────
        public ActionResult XetDuyetYeuCau()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            var role = Session["UserRole"];
            if (role == null || Convert.ToInt32(role) != 4) return RedirectToAction("Index", "Home");
            ViewBag.ChoDuyet = new System.Collections.Generic.List<DeXuatViewModel>();
            ViewBag.LichSu   = new System.Collections.Generic.List<DeXuatViewModel>();
            try
            {
                var (choDuyet, lichSu) = _svc.GetDeXuatForBGH();
                ViewBag.ChoDuyet = choDuyet;
                ViewBag.LichSu = lichSu;
            }
            catch (Exception ex) { ViewBag.Error = "Lỗi tải dữ liệu: " + ex.Message; }
            return View();
        }

        [HttpPost]
        public ActionResult DuyetDeXuat(string id, string action, string ghiChu)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.DuyetDeXuatBGH(id, action, ghiChu, CurrentUser); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // ── KHTC: Phê duyệt ngân sách ────────────────────────
        public ActionResult PheDuyetNganSach()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != 3) return RedirectToAction("Index", "Home");
            try { return View(_svc.GetDeXuatForKHTC()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new System.Collections.Generic.List<DeXuatViewModel>()); }
        }

        [HttpPost]
        public ActionResult XuLyNganSach(string id, string action, string ghiChu)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.XuLyNganSachKHTC(id, action, ghiChu, CurrentUser); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        public ActionResult QuanLyChiPhi()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != 3) return RedirectToAction("Index", "Home");
            try { return View(_svc.GetChiPhiList()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new System.Collections.Generic.List<ChiPhiViewModel>()); }
        }

        [HttpPost]
        public ActionResult CapNhatChiPhi(string id, decimal chiPhiMoi)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.CapNhatChiPhi(id, chiPhiMoi); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpGet]
        public JsonResult GetDeXuatDetail(string id)
        {
            try { return Json(new { ok = true, data = _svc.GetChiTiet(id) }, JsonRequestBehavior.AllowGet); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpGet]
        public JsonResult GetLichSuSuaChua(string id)
        {
            try { return Json(new { ok = true, data = _svc.GetLichSuSuaChua(id) }, JsonRequestBehavior.AllowGet); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
    }
}
