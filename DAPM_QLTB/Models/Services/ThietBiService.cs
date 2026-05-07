using System.Collections.Generic;
using System.Web.Mvc;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class ThietBiService
    {
        private readonly ThietBiRepository _repo = new ThietBiRepository();

        /// <summary>Lấy danh sách thiết bị + tất cả dropdown cho trang quản lý.</summary>
        public (List<ThietBiViewModel> list, List<SelectListItem> danhMuc, List<SelectListItem> khoa,
                List<SelectListItem> coSo, List<KhuViewModel> khu, List<SelectListItem> ncc, List<PhongViewModel> phong)
            GetAllForManagement() => _repo.GetAllForManagement();

        public ThietBiViewModel GetDetail(string id) => _repo.GetDetail(id);
        public object GetById(string id) => _repo.GetById(id);
        public (bool ok, string msg) Save(ThietBiViewModel tb) => _repo.Save(tb);
        public (bool ok, string msg) Delete(string id) => _repo.Delete(id);
        public string GenerateNewId() => _repo.GenerateNewId();
    }
}
