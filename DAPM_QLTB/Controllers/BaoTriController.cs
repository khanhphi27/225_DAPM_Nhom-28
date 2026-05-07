using System;
using System.Web.Mvc;
using QLTB.Models;
using QLTB.Models.Services;

namespace QLTB.Controllers
{
    public class BaoTriController : Controller
    {
        private readonly BaoTriService _svc = new BaoTriService();

        private ActionResult CheckAuth()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return null;
        }
        private string CurrentUser => Session["UserId"]?.ToString() ?? "csvc";

        public ActionResult LapKeHoach()
        {
            var r = CheckAuth(); if (r != null) return r;
            try { return View(_svc.GetLapKeHoachData()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new LapKeHoachPageViewModel()); }
        }

        [HttpPost]
        public ActionResult LuuKeHoach(string idKeHoach, string loaiKeHoach, string donViThucHien,
            string ngayDuKienHT, string chiPhiDuKien, string ghiChu,
            string[] thietBiNos, string[] baoHongNos, string[] nguonGocs, string[] ghiChuCTs)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try
            {
                var (ok, msg) = _svc.LuuKeHoach(idKeHoach, loaiKeHoach, donViThucHien, ngayDuKienHT,
                    chiPhiDuKien, ghiChu, thietBiNos, baoHongNos, nguonGocs, ghiChuCTs, CurrentUser);
                return Json(new { ok, msg });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public ActionResult XoaKeHoach(string id)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try { var (ok, msg) = _svc.XoaKeHoach(id); return Json(new { ok, msg }); }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        public ActionResult GhiNhan()
        {
            var r = CheckAuth(); if (r != null) return r;
            try { return View(_svc.GetGhiNhanData()); }
            catch (Exception ex) { ViewBag.Error = ex.Message; return View(new GhiNhanPageViewModel()); }
        }

        [HttpPost]
        public ActionResult LuuGhiNhan(string idGhiNhan, string keHoachNo, string chiTietKeHoachNo,
            string ngayThucHien, string ketQua, string chiPhiThucTe, string trangThaiSauSua)
        {
            var r = CheckAuth(); if (r != null) return Json(new { ok = false, msg = "Chưa đăng nhập." });
            try
            {
                var (ok, msg, trangThaiKH) = _svc.LuuGhiNhan(idGhiNhan, keHoachNo, chiTietKeHoachNo,
                    ngayThucHien, ketQua, chiPhiThucTe, trangThaiSauSua);
                return Json(new { ok, trangThaiKH, msg });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }
    }
}
