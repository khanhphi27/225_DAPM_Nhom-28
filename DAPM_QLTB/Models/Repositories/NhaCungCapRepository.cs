using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace QLTB.Models.Repositories
{
    public class NhaCungCapRepository
    {
        public List<NhaCungCapViewModel> GetAll()
        {
            var list = new List<NhaCungCapViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT ncc.ID_NhaCC, ncc.TenNhaCC, ncc.LoaiDichVu, ncc.DiaChi, ncc.Sdt,
                           COUNT(tb.ID_ThietBi) AS SoThietBi
                    FROM NHACUNGCAP ncc
                    LEFT JOIN THIETBI tb ON tb.NhaCCNo = ncc.ID_NhaCC
                    GROUP BY ncc.ID_NhaCC, ncc.TenNhaCC, ncc.LoaiDichVu, ncc.DiaChi, ncc.Sdt
                    ORDER BY ncc.TenNhaCC";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new NhaCungCapViewModel
                        {
                            ID_NhaCC = rd["ID_NhaCC"].ToString(),
                            TenNhaCC = rd["TenNhaCC"].ToString(),
                            LoaiDichVu = rd["LoaiDichVu"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["LoaiDichVu"]),
                            DiaChi = rd["DiaChi"]?.ToString() ?? "",
                            Sdt = rd["Sdt"]?.ToString() ?? "",
                            SoThietBi = Convert.ToInt32(rd["SoThietBi"])
                        });
            }
            return list;
        }

        public object GetById(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"SELECT ID_NhaCC, TenNhaCC, LoaiDichVu, DiaChi, Sdt FROM NHACUNGCAP WHERE ID_NhaCC = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                            return new
                            {
                                id = rd["ID_NhaCC"].ToString(),
                                ten = rd["TenNhaCC"].ToString(),
                                loaiDichVu = rd["LoaiDichVu"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["LoaiDichVu"]),
                                diaChi = rd["DiaChi"]?.ToString() ?? "",
                                sdt = rd["Sdt"]?.ToString() ?? ""
                            };
                }
            }
            return null;
        }

        public (bool ok, string msg) Save(string id, string ten, int? loaiDichVu, string diaChi, string sdt)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Vui lòng nhập mã nhà cung cấp.");
            if (string.IsNullOrWhiteSpace(ten)) return (false, "Vui lòng nhập tên nhà cung cấp.");

            id = id.Trim();
            ten = ten.Trim();
            bool exists = false;

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NHACUNGCAP WHERE ID_NhaCC = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                string sql = exists
                    ? @"UPDATE NHACUNGCAP SET TenNhaCC = @ten, LoaiDichVu = @loai, DiaChi = @diaChi, Sdt = @sdt WHERE ID_NhaCC = @id"
                    : @"INSERT INTO NHACUNGCAP (ID_NhaCC, TenNhaCC, LoaiDichVu, DiaChi, Sdt) VALUES (@id, @ten, @loai, @diaChi, @sdt)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@ten", ten);
                    cmd.Parameters.AddWithValue("@loai", (object)loaiDichVu ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@diaChi", string.IsNullOrWhiteSpace(diaChi) ? (object)DBNull.Value : diaChi.Trim());
                    cmd.Parameters.AddWithValue("@sdt", string.IsNullOrWhiteSpace(sdt) ? (object)DBNull.Value : sdt.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
            return (true, exists ? "Cập nhật nhà cung cấp thành công!" : "Thêm nhà cung cấp thành công!");
        }

        public (bool ok, string msg) Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Thiếu mã nhà cung cấp.");

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                int soTB;
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM THIETBI WHERE NhaCCNo = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id.Trim());
                    soTB = Convert.ToInt32(cmd.ExecuteScalar());
                }
                if (soTB > 0)
                    return (false, "Không thể xóa vì nhà cung cấp đang được gắn với thiết bị.");

                using (var cmd = new SqlCommand("DELETE FROM NHACUNGCAP WHERE ID_NhaCC = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
            return (true, "Xóa nhà cung cấp thành công!");
        }
    }
}
