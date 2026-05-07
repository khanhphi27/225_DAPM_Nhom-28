using System.Collections.Generic;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class DanhMucService
    {
        private readonly DanhMucRepository _repo = new DanhMucRepository();

        public List<DanhMucViewModel> GetAll() => _repo.GetAll();
        public object GetById(string id) => _repo.GetById(id);
        public (bool ok, string msg) Save(string id, string ten, string moTa) => _repo.Save(id, ten, moTa);
        public (bool ok, string msg) Delete(string id) => _repo.Delete(id);
    }
}
