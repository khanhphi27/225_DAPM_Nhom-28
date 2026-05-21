using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace QLTB.Models.Repositories
{
    public class KiemKeRepository
    {
        /// <summary>Dữ liệu trang KiemKeTaiSan.</summary>
        public KiemKeTaiSanViewModel GetKiemKeTaiSan()
        {
            var vm = new KiemKeTaiSanViewModel();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, ISNULL(tb.TrangThaiTB,'') AS TrangThaiTB,
                           ISNULL(kp.TenPhongBanKhoa,'') AS TenKhoa, ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                           1 AS SoLuongHeThong,
                           ckk.SoLuongThucTe, ckk.TinhTrangThucTe, ckk.GhiChu, kk.NgayKiemKe
                    FROM THIETBI tb
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                    LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                    LEFT JOIN CHITIET_KIEMKE ckk ON ckk.ThietBiNo=tb.ID_ThietBi
                    LEFT JOIN KIEMKE kk ON kk.ID_KiemKe=ckk.KiemKeNo
                    ORDER BY kp.TenPhongBanKhoa, tb.TenTB, tb.ID_ThietBi";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                    {
                        var row = new ThietBiKiemKeRow
                        {
                            ID_ThietBi = rd["ID_ThietBi"].ToString(),
                            TenTB = rd["TenTB"].ToString(),
                            TrangThaiTB = rd["TrangThaiTB"].ToString(),
                            TenKhoa = rd["TenKhoa"].ToString(),
                            TenDanhMuc = rd["TenDanhMuc"].ToString(),
                            SoLuongHeThong = Convert.ToInt32(rd["SoLuongHeThong"]),
                            SoLuongThucTe = rd["SoLuongThucTe"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SoLuongThucTe"]),
                            TinhTrangThucTe = rd["TinhTrangThucTe"]?.ToString(),
                            GhiChu = rd["GhiChu"]?.ToString(),
                            NgayKiemKe = rd["NgayKiemKe"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NgayKiemKe"])
                        };
                        vm.DanhSachThietBi.Add(row);
                        vm.TongThietBi++;
                        if (row.SoLuongThucTe.HasValue) vm.DaKiemKe++; else vm.ChuaKiemKe++;
                    }
            }

            // Build nhóm theo khoa → tên thiết bị
            foreach (var grpKhoa in vm.DanhSachThietBi.GroupBy(x => x.TenKhoa))
            {
                var nhomKhoa = new NhomKhoaKiemKe { TenKhoa = grpKhoa.Key };
                foreach (var grpTen in grpKhoa.GroupBy(x => x.TenTB))
                {
                    var items    = grpTen.ToList();
                    int slSoSach = items.Count;   // tổng số TB cùng tên trong khoa
                    bool daKiem  = items.Any(x => x.NgayKiemKe.HasValue);
                    // SL thực tế = số TB đã kiểm kê mà TinhTrangThucTe != "Mất"
                    int slThucTe = items.Count(x =>
                        x.NgayKiemKe.HasValue &&
                        !string.Equals(x.TinhTrangThucTe, "Mất", StringComparison.OrdinalIgnoreCase));
                    string ketQua = !daKiem              ? "Chưa kiểm"
                                  : slThucTe >= slSoSach ? "Đủ" : "Thiếu";
                    var ghiChuList = items.Where(x => !string.IsNullOrWhiteSpace(x.GhiChu))
                                         .Select(x => x.GhiChu).Distinct().ToList();
                    nhomKhoa.NhomThietBi.Add(new NhomThietBiKiemKe
                    {
                        TenTB         = grpTen.Key,
                        TenDanhMuc    = items.First().TenDanhMuc,
                        TenKhoa       = grpKhoa.Key,
                        SoLuongSoSach = slSoSach,
                        SoLuongThucTe = daKiem ? (int?)slThucTe : null,
                        KetQuaKiemKe  = ketQua,
                        GhiChu        = ghiChuList.Any() ? string.Join("; ", ghiChuList) : "",
                        ChiTiet       = items
                    });
                }
                vm.NhomTheoKhoa.Add(nhomKhoa);
            }

            return vm;
        }

        /// <summary>Dữ liệu trang TaoPhieuKiemKe.</summary>
        public TaoPhieuKiemKeViewModel GetTaoPhieuData(string nguoiTao)
        {
            var vm = new TaoPhieuKiemKeViewModel
            {
                NgayKiemKe = DateTime.Now,
                NguoiTao = nguoiTao
            };
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                // Sinh ID mới
                using (var cmd = new SqlCommand("SELECT 'KK' + RIGHT('00000' + CAST(ISNULL(MAX(TRY_CAST(SUBSTRING(ID_KiemKe,3,LEN(ID_KiemKe)) AS INT)),0)+1 AS VARCHAR), 5) FROM KIEMKE", conn))
                    vm.ID_KiemKe = cmd.ExecuteScalar()?.ToString() ?? "KK00001";

                // Thiết bị chưa kiểm kê
                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, ISNULL(tb.TrangThaiTB,'') AS TrangThaiTB,
                           ISNULL(kp.TenPhongBanKhoa,'') AS TenKhoa, ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc
                    FROM THIETBI tb
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                    LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                    WHERE NOT EXISTS (SELECT 1 FROM CHITIET_KIEMKE ckk WHERE ckk.ThietBiNo=tb.ID_ThietBi)
                    ORDER BY kp.TenPhongBanKhoa, tb.TenTB, tb.ID_ThietBi";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        vm.DanhSachChuaKiem.Add(new ItemTaoKiemKe
                        {
                            ID_ThietBi = rd["ID_ThietBi"].ToString(),
                            TenTB = rd["TenTB"].ToString(),
                            TrangThaiTB = rd["TrangThaiTB"].ToString(),
                            TenKhoa = rd["TenKhoa"].ToString(),
                            TenDanhMuc = rd["TenDanhMuc"].ToString(),
                            SoLuongHeThong = 1
                        });
            }

            // Build nhóm theo khoa → tên thiết bị
            foreach (var grpKhoa in vm.DanhSachChuaKiem.GroupBy(x => x.TenKhoa))
            {
                var nhomKhoa = new NhomKhoaTaoPhieu { TenKhoa = grpKhoa.Key };
                foreach (var grpTen in grpKhoa.GroupBy(x => x.TenTB))
                {
                    var items = grpTen.ToList();
                    nhomKhoa.NhomThietBi.Add(new NhomThietBiTaoPhieu
                    {
                        TenTB         = grpTen.Key,
                        TenDanhMuc    = grpTen.First().TenDanhMuc,
                        TenKhoa       = grpKhoa.Key,
                        SoLuongSoSach = items.Count,
                        // Chưa kiểm kê nên ước tính: TB không có trạng thái "Hỏng" hoặc "Mất"
                        SoLuongThucTe = items.Count(x =>
                            !string.Equals(x.TrangThaiTB, "Hỏng", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(x.TrangThaiTB, "Mất",  StringComparison.OrdinalIgnoreCase)),
                        ChiTiet       = items
                    });
                }
                vm.NhomTheoKhoa.Add(nhomKhoa);
            }

            return vm;
        }

        /// <summary>Lưu phiếu kiểm kê — chỉ lưu các thiết bị có tình trạng được chọn (đã tick).</summary>
        public (bool ok, string msg) HoanTatKiemKe(string idKiemKe, string nguoiTao, List<ItemTaoKiemKe> chiTiet)
        {
            if (chiTiet == null || chiTiet.Count == 0)
                return (false, "Không có thiết bị nào để kiểm kê.");

            // Chỉ lấy những TB đã được tick (có TinhTrangThucTe)
            var daTick = chiTiet.Where(x =>
                !string.IsNullOrWhiteSpace(x.ID_ThietBi) &&
                !string.IsNullOrWhiteSpace(x.TinhTrangThucTe)).ToList();

            if (daTick.Count == 0)
                return (false, "Không có thiết bị nào được chọn để kiểm kê.");

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"INSERT INTO KIEMKE (ID_KiemKe,NgayKiemKe,NguoiThucHienNo,TrangThai,GhiChu)
                            VALUES (@id,@ngay,@nguoi,N'Hoàn thành',NULL)", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id",    idKiemKe);
                            cmd.Parameters.AddWithValue("@ngay",  DateTime.Now);
                            cmd.Parameters.AddWithValue("@nguoi", nguoiTao ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        // Tính SL sổ sách và SL thực tế theo nhóm (tên TB + khoa) từ toàn bộ danh sách đã tick
                        // SL thực tế = số TB trong nhóm mà TinhTrangThucTe != "Mất"
                        var nhomDict = daTick
                            .GroupBy(x => x.TenKhoa + "|" + x.TenTB)
                            .ToDictionary(
                                g => g.Key,
                                g => new {
                                    SoSach = chiTiet.Count(x => x.TenKhoa == g.First().TenKhoa && x.TenTB == g.First().TenTB),
                                    ThucTe = g.Count(x => !string.Equals(x.TinhTrangThucTe, "Mất", StringComparison.OrdinalIgnoreCase))
                                });

                        foreach (var item in daTick)
                        {
                            string ctId;
                            using (var cmdId = new SqlCommand("SELECT LEFT(REPLACE(NEWID(),'-',''),10)", conn, tran))
                            {
                                ctId = cmdId.ExecuteScalar().ToString();
                            }

                            var key  = item.TenKhoa + "|" + item.TenTB;
                            var nhom = nhomDict.ContainsKey(key) ? nhomDict[key] : null;

                            using (var cmd = new SqlCommand(@"INSERT INTO CHITIET_KIEMKE
                                (ID_ChiTietKK,KiemKeNo,ThietBiNo,SoLuongHeThong,SoLuongThucTe,TinhTrangThucTe,GhiChu)
                                VALUES (@id,@kk,@tb,@slht,@sltt,@tt,@gc)", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@id",   ctId);
                                cmd.Parameters.AddWithValue("@kk",   idKiemKe);
                                cmd.Parameters.AddWithValue("@tb",   item.ID_ThietBi);
                                cmd.Parameters.AddWithValue("@slht", nhom != null ? nhom.SoSach : 1);
                                cmd.Parameters.AddWithValue("@sltt", nhom != null ? nhom.ThucTe : 1);
                                cmd.Parameters.AddWithValue("@tt",   item.TinhTrangThucTe);
                                cmd.Parameters.AddWithValue("@gc",   (object)item.GhiChu ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
            return (true, "Đã lưu phiếu kiểm kê " + idKiemKe + " (" + daTick.Count + " thiết bị)");
        }
        /// <summary>Kiểm kê định kỳ — xóa toàn bộ kết quả kiểm kê, reset về chưa kiểm.</summary>
        public (bool ok, string msg) KiemKeDinhKy()
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Xóa chi tiết trước (FK), sau đó xóa phiếu
                        using (var cmd = new SqlCommand("DELETE FROM CHITIET_KIEMKE", conn, tran))
                            cmd.ExecuteNonQuery();
                        using (var cmd = new SqlCommand("DELETE FROM KIEMKE", conn, tran))
                            cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
            return (true, "Đã reset toàn bộ kết quả kiểm kê. Hệ thống sẵn sàng cho đợt kiểm kê mới.");
        }
    }
}
