using System.Collections.Generic;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class NhaCungCapService
    {
        private readonly NhaCungCapRepository _repo = new NhaCungCapRepository();

        public List<NhaCungCapViewModel> GetAll() => _repo.GetAll();
        public object GetById(string id) => _repo.GetById(id);
        public (bool ok, string msg) Save(string id, string ten, int? loaiDichVu, string diaChi, string sdt) => _repo.Save(id, ten, loaiDichVu, diaChi, sdt);
        public (bool ok, string msg) Delete(string id) => _repo.Delete(id);
    }
}
