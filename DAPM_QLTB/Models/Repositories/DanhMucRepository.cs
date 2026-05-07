using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace QLTB.Models.Repositories
{
    public class DanhMucRepository
    {
        public List<DanhMucViewModel> GetAll()
        {
            var list = new List<DanhMucViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT dm.ID_DanhMuc, dm.TenDanhMuc, dm.MoTa,
                           COUNT(tb.ID_ThietBi) AS SoThietBi
                    FROM DANHMUC dm
                    LEFT JOIN THIETBI tb ON tb.DanhMucNo = dm.ID_DanhMuc
                    GROUP BY dm.ID_DanhMuc, dm.TenDanhMuc, dm.MoTa
                    ORDER BY dm.TenDanhMuc";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new DanhMucViewModel
                        {
                            ID_DanhMuc = rd["ID_DanhMuc"].ToString(),
                            TenDanhMuc = rd["TenDanhMuc"].ToString(),
                            MoTa = rd["MoTa"]?.ToString() ?? "",
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
                const string sql = @"SELECT ID_DanhMuc, TenDanhMuc, MoTa FROM DANHMUC WHERE ID_DanhMuc = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                            return new
                            {
                                id = rd["ID_DanhMuc"].ToString(),
                                ten = rd["TenDanhMuc"].ToString(),
                                moTa = rd["MoTa"]?.ToString() ?? ""
                            };
                }
            }
            return null;
        }

        /// <summary>Insert hoặc Update danh mục. Trả về (success, message).</summary>
        public (bool ok, string msg) Save(string id, string ten, string moTa)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Vui lòng nhập mã danh mục.");
            if (string.IsNullOrWhiteSpace(ten)) return (false, "Vui lòng nhập tên danh mục.");

            id = id.Trim();
            ten = ten.Trim();
            bool exists = false;

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM DANHMUC WHERE ID_DanhMuc = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                string sql = exists
                    ? @"UPDATE DANHMUC SET TenDanhMuc = @ten, MoTa = @moTa WHERE ID_DanhMuc = @id"
                    : @"INSERT INTO DANHMUC (ID_DanhMuc, TenDanhMuc, MoTa) VALUES (@id, @ten, @moTa)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@ten", ten);
                    cmd.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(moTa) ? (object)DBNull.Value : moTa.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
            return (true, exists ? "Cập nhật danh mục thành công!" : "Thêm danh mục thành công!");
        }

        /// <summary>Xóa danh mục. Trả về (success, message).</summary>
        public (bool ok, string msg) Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Thiếu mã danh mục.");

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                int soTB;
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM THIETBI WHERE DanhMucNo = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id.Trim());
                    soTB = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (soTB > 0)
                    return (false, "Không thể xóa vì danh mục đang được gắn với thiết bị.");

                using (var cmd = new SqlCommand("DELETE FROM DANHMUC WHERE ID_DanhMuc = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
            return (true, "Xóa danh mục thành công!");
        }
    }
}
