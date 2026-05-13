using System.Collections.Generic;
using System.Web.Mvc;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class PhongService
    {
        private readonly PhongRepository _repo = new PhongRepository();

        public List<PhongViewModel> GetAll() => _repo.GetAll();
        public List<SelectListItem> GetKhuVucDropdown() => _repo.GetKhuVucDropdown();
        public object GetById(string id) => _repo.GetById(id);
        public (bool ok, string msg) Create(string id, string ten, string khuVucNo, int? sucChua) => _repo.Create(id, ten, khuVucNo, sucChua);
        public (bool ok, string msg) Update(string id, string ten, string khuVucNo, int? sucChua) => _repo.Update(id, ten, khuVucNo, sucChua);
        public (bool ok, string msg) Delete(string id) => _repo.Delete(id);
    }
}
