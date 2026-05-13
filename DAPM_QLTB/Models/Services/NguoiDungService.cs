using System.Collections.Generic;
using System.Web.Mvc;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class NguoiDungService
    {
        private readonly NguoiDungRepository _repo = new NguoiDungRepository();

        public List<NguoiDungViewModel> GetAll() => _repo.GetAll();
        public object GetById(string id) => _repo.GetById(id);

        public (bool ok, string msg) Create(string id, string hoTen, string email,
            string matKhau, string khoaBanNo, string vaiTroNo, bool trangThai)
            => _repo.Create(id, hoTen, email, matKhau, khoaBanNo, vaiTroNo, trangThai);

        public (bool ok, string msg) Update(string id, string hoTen, string email,
            string matKhauMoi, string khoaBanNo, string vaiTroNo, bool trangThai)
            => _repo.Update(id, hoTen, email, matKhauMoi, khoaBanNo, vaiTroNo, trangThai);

        public (bool ok, string msg) Delete(string id, string currentUserId)
            => _repo.Delete(id, currentUserId);

        public List<SelectListItem> GetVaiTroDropdown() => _repo.GetVaiTroDropdown();
    }
}
