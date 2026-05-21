using System.Collections.Generic;
using System.Web.Mvc;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class KhoaService
    {
        private readonly KhoaRepository _repo = new KhoaRepository();

        public List<KhoaPhongBanViewModel> GetAll() => _repo.GetAll();
        public object GetById(string id) => _repo.GetById(id);
        public object GetChiTiet(string id) => _repo.GetChiTiet(id);
        public (bool ok, string msg) Create(string id, string ten) => _repo.Create(id, ten);
        public (bool ok, string msg) Update(string id, string ten) => _repo.Update(id, ten);
        public (bool ok, string msg) Delete(string id) => _repo.Delete(id);
        public List<SelectListItem> GetDropdownList() => _repo.GetDropdownList();
    }
}
