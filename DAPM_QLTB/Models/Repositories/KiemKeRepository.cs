using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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
                    ORDER BY tb.TenTB";
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
                using (var cmd = new SqlCommand("SELECT 'KK' + RIGHT('00000' + CAST(ISNULL(MAX(CAST(SUBSTRING(ID_KiemKe,3,LEN(ID_KiemKe)) AS INT)),0)+1 AS VARCHAR), 5) FROM KIEMKE", conn))
                    vm.ID_KiemKe = cmd.ExecuteScalar()?.ToString() ?? "KK00001";

                // Thiết bị chưa kiểm kê
                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, ISNULL(tb.TrangThaiTB,'') AS TrangThaiTB,
                           ISNULL(kp.TenPhongBanKhoa,'') AS TenKhoa, ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc
                    FROM THIETBI tb
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan=tb.KhoaPhongBan
                    LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc=tb.DanhMucNo
                    WHERE NOT EXISTS (SELECT 1 FROM CHITIET_KIEMKE ckk WHERE ckk.ThietBiNo=tb.ID_ThietBi)
                    ORDER BY tb.TenTB";
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
            return vm;
        }

        /// <summary>Lưu phiếu kiểm kê.</summary>
        public (bool ok, string msg) HoanTatKiemKe(TaoPhieuKiemKeViewModel model)
        {
            if (model.DanhSachChuaKiem == null || model.DanhSachChuaKiem.Count == 0)
                return (false, "Không có thiết bị nào để kiểm kê.");

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
                            cmd.Parameters.AddWithValue("@id", model.ID_KiemKe);
                            cmd.Parameters.AddWithValue("@ngay", model.NgayKiemKe);
                            cmd.Parameters.AddWithValue("@nguoi", model.NguoiTao ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var item in model.DanhSachChuaKiem)
                        {
                            if (string.IsNullOrWhiteSpace(item.ID_ThietBi)) continue;
                            string ctId;
                            using (var cmdId = new SqlCommand("SELECT LEFT(REPLACE(NEWID(),'-',''),10)", conn, tran))
                                ctId = cmdId.ExecuteScalar().ToString();

                            using (var cmd = new SqlCommand(@"INSERT INTO CHITIET_KIEMKE (ID_ChiTietKK,KiemKeNo,ThietBiNo,SoLuongHeThong,SoLuongThucTe,TinhTrangThucTe,GhiChu)
                                VALUES (@id,@kk,@tb,@slht,@sltt,@tt,@gc)", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@id", ctId);
                                cmd.Parameters.AddWithValue("@kk", model.ID_KiemKe);
                                cmd.Parameters.AddWithValue("@tb", item.ID_ThietBi);
                                cmd.Parameters.AddWithValue("@slht", item.SoLuongHeThong);
                                cmd.Parameters.AddWithValue("@sltt", item.SoLuongThucTe.HasValue ? (object)item.SoLuongThucTe.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@tt", (object)item.TinhTrangThucTe ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@gc", (object)item.GhiChu ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
            return (true, "Đã lưu phiếu kiểm kê " + model.ID_KiemKe);
        }
    }
}
