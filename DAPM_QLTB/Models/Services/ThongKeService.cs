using System.Collections.Generic;
using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class ThongKeService
    {
        private readonly ThongKeRepository _repo = new ThongKeRepository();

        // BGH
        public BGHDashboardViewModel GetBGHDashboard() => _repo.GetBGHDashboard();
        public ThongKeTaiSanViewModel GetThongKeTaiSan() => _repo.GetThongKeTaiSan();
        public List<TheDoiThietBiViewModel> GetTheoDoi() => _repo.GetTheoDoi();
        public List<object> GetLichSuDuyetTheoThietBi(string maTB, string tenTB) => _repo.GetLichSuDuyetTheoThietBi(maTB, tenTB);

        // KHTC
        public KHTCDashboardViewModel GetKHTCDashboard() => _repo.GetKHTCDashboard();
        public List<BaoCaoTaiSanViewModel> GetBaoCaoTaiSan() => _repo.GetBaoCaoTaiSan();
    }
}
