using System.Collections.Generic;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class KiemKeService
    {
        private readonly KiemKeRepository _repo = new KiemKeRepository();

        public KiemKeTaiSanViewModel GetKiemKeTaiSan() => _repo.GetKiemKeTaiSan();
        public TaoPhieuKiemKeViewModel GetTaoPhieuData(string nguoiTao) => _repo.GetTaoPhieuData(nguoiTao);
        public (bool ok, string msg) HoanTatKiemKe(string idKiemKe, string nguoiTao, List<ItemTaoKiemKe> chiTiet) => _repo.HoanTatKiemKe(idKiemKe, nguoiTao, chiTiet);
        public (bool ok, string msg) KiemKeDinhKy() => _repo.KiemKeDinhKy();
    }
}
