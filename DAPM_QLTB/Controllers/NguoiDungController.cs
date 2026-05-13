using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class NguoiDungController : Controller
    {
        private readonly NguoiDungService _svc = new NguoiDungService();
        private readonly KhoaService _khoaSvc = new KhoaService();

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
            try
            {
                ViewBag.KhoaList = _khoaSvc.GetDropdownList();
                ViewBag.VaiTroList = _svc.GetVaiTroDropdown();
                return View(_svc.GetAll());
            }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new System.Collections.Generic.List<NguoiDungViewModel>()); }
        }

        [HttpGet]
        public JsonResult GetNguoiDung(string id)
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
        public JsonResult TaoNguoiDung(string id, string hoTen, string email, string matKhau, string khoaBanNo, string vaiTroNo, bool trangThai)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Create(id, hoTen, email, matKhau, khoaBanNo, vaiTroNo, trangThai); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public JsonResult SuaNguoiDung(string id, string hoTen, string email, string matKhauMoi, string khoaBanNo, string vaiTroNo, bool trangThai)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Update(id, hoTen, email, matKhauMoi, khoaBanNo, vaiTroNo, trangThai); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public JsonResult XoaNguoiDung(string id)
        {
            if (Session["UserId"] == null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.Delete(id, Session["UserId"]?.ToString()); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }
    }
}
