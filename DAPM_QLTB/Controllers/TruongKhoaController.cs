using System;
using System.Data;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class TruongKhoaController : Controller
    {
        private readonly TruongKhoaService _svc = new TruongKhoaService();
        private readonly DeXuatService _deXuatSvc = new DeXuatService();

        private string CurrentUserId => Session["UserId"]?.ToString() ?? "";

        private ActionResult RequireRole()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            if (Session["UserRole"] == null || (int)Session["UserRole"] != 1) return RedirectToAction("Index", "Home");
            return null;
        }

        public ActionResult Index() => View();

        // ===================== ĐỀ XUẤT MUA SẮM =====================
        [HttpGet]
        public ActionResult DeXuatMuaSam()
        {
            var r = RequireRole(); if (r != null) return r;
            try { return View("GuiDeXuat", _svc.GetDeXuatByUser(CurrentUserId)); }
            catch (Exception ex) { ViewBag.Error = "Lỗi tải dữ liệu: " + ex.Message; return View("GuiDeXuat", new DataTable()); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiDeXuat(string[] tenTB, int[] soluong, decimal[] gia, string[] donvi, string mota)
        {
            if (tenTB == null || tenTB.Length == 0)
            {
                ViewBag.Error = "Vui lòng nhập ít nhất 1 thiết bị.";
                return RedirectToAction("DeXuatMuaSam");
            }

            try
            {
                var (ok, msg, _, _) = _svc.GuiDeXuat(CurrentUserId, mota, tenTB, soluong, gia, donvi);
                TempData[ok ? "Success" : "Error"] = msg;
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi: " + ex.Message; }
            return RedirectToAction("DeXuatMuaSam");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChinhSuaDeXuat(string idDX, string[] tenTB, int[] soluong, decimal[] gia, string[] donvi, string mota)
        {
            if (string.IsNullOrEmpty(idDX) || tenTB == null || tenTB.Length == 0)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("DeXuatMuaSam");
            }

            try
            {
                var (ok, msg) = _svc.ChinhSuaDeXuat(idDX, CurrentUserId, mota, tenTB, soluong, gia, donvi);
                TempData[ok ? "Success" : "Error"] = msg;
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi: " + ex.Message; }
            return RedirectToAction("DeXuatMuaSam");
        }

        // ===================== DANH SÁCH THIẾT BỊ =====================
        public ActionResult DanhSachThietBi()
        {
            var r = RequireRole(); if (r != null) return r;
            try
            {
                var (khoaBanNo, tenKhoa) = _svc.GetKhoaPhongBanByUser(CurrentUserId);
                if (string.IsNullOrWhiteSpace(khoaBanNo))
                {
                    ViewBag.Error = "Không xác định được khoa/phòng ban của người dùng hiện tại.";
                    return View(new DataTable());
                }
                ViewBag.TenKhoaPhongBan = tenKhoa;
                return View(_svc.GetThietBiByKhoa(khoaBanNo));
            }
            catch (Exception ex) { ViewBag.Error = "Lỗi: " + ex.Message; return View(new DataTable()); }
        }

        [HttpPost]
        public ActionResult BaoHong(string id, string mota)
        {
            var r = RequireRole(); if (r != null) return r;
            try
            {
                var (khoaBanNo, _) = _svc.GetKhoaPhongBanByUser(CurrentUserId);
                if (string.IsNullOrWhiteSpace(khoaBanNo))
                {
                    TempData["Error"] = "Không xác định được khoa/phòng ban.";
                    return RedirectToAction("DanhSachThietBi");
                }
                var (ok, msg) = _svc.BaoHong(id, mota, CurrentUserId, khoaBanNo);
                TempData[ok ? "Success" : "Error"] = msg;
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi báo hỏng: " + ex.Message; }
            return RedirectToAction("DanhSachThietBi");
        }

        // Redirect link cũ
        public ActionResult GuiDeXuat() => RedirectToAction("DeXuatMuaSam");
        public ActionResult XemDeXuat() => RedirectToAction("DeXuatMuaSam");

        // Ajax: chi tiết đề xuất
        [HttpGet]
        public JsonResult GetChiTietDeXuat(string id)
        {
            try { return Json(new { ok = true, data = _deXuatSvc.GetChiTiet(id) }, JsonRequestBehavior.AllowGet); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
    }
}
