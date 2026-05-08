using QLTB.Models.Repositories;

namespace QLTB.Models.Services
{
    public class BaoTriService
    {
        private readonly BaoTriRepository _repo = new BaoTriRepository();

        public LapKeHoachPageViewModel GetLapKeHoachData() => _repo.GetLapKeHoachData();

        public (bool ok, string msg) LuuKeHoach(string idKH, string loai, string donVi,
            string ngayDuKienHT, string chiPhi, string ghiChu,
            string[] thietBiNos, string[] baoHongNos, string[] nguonGocs, string[] ghiChuCTs, string nguoiLapId)
            => _repo.LuuKeHoach(idKH, loai, donVi, ngayDuKienHT, chiPhi, ghiChu, thietBiNos, baoHongNos, nguonGocs, ghiChuCTs, nguoiLapId);

        public (bool ok, string msg) XoaKeHoach(string id) => _repo.XoaKeHoach(id);
        public object GetKeHoachDetail(string id) => _repo.GetKeHoachDetail(id);

        public GhiNhanPageViewModel GetGhiNhanData() => _repo.GetGhiNhanData();

        public (bool ok, string msg, string trangThaiKH) LuuGhiNhan(string idGhiNhan, string keHoachNo,
            string chiTietKeHoachNo, string ngayThucHien, string ketQua, string chiPhiThucTe, string trangThaiSauSua)
            => _repo.LuuGhiNhan(idGhiNhan, keHoachNo, chiTietKeHoachNo, ngayThucHien, ketQua, chiPhiThucTe, trangThaiSauSua);
    }
}
