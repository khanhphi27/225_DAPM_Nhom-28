using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QLTB.Models.Repositories
{
    public class PhongRepository
    {
        public List<PhongViewModel> GetAll()
        {
            var list = new List<PhongViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT p.ID_Phong, p.TenPhong, p.KhuVucNo, p.SucChua,
                           ISNULL(kv.TenKhuVuc, N'') AS TenKhuVuc,
                           COUNT(DISTINCT pt.ThietBiNo) AS SoThietBi
                    FROM PHONG p
                    LEFT JOIN KHUVUC kv ON kv.ID_KhuVuc = p.KhuVucNo
                    LEFT JOIN PHONG_THIETBI pt ON pt.PhongNo = p.ID_Phong
                    GROUP BY p.ID_Phong, p.TenPhong, p.KhuVucNo, p.SucChua, kv.TenKhuVuc
                    ORDER BY kv.TenKhuVuc, p.TenPhong";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new PhongViewModel
                        {
                            ID_Phong = rd["ID_Phong"].ToString(),
                            TenPhong = rd["TenPhong"].ToString(),
                            KhuVucNo = rd["KhuVucNo"]?.ToString() ?? "",
                            TenKhuVuc = rd["TenKhuVuc"].ToString(),
                            SucChua = rd["SucChua"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SucChua"]),
                            SoThietBi = Convert.ToInt32(rd["SoThietBi"])
                        });
            }
            return list;
        }

        public List<SelectListItem> GetKhuVucDropdown()
        {
            var list = new List<SelectListItem>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID_KhuVuc, TenKhuVuc FROM KHUVUC ORDER BY TenKhuVuc", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new SelectListItem { Value = rd["ID_KhuVuc"].ToString(), Text = rd["TenKhuVuc"].ToString() });
            }
            return list;
        }

        public object GetById(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID_Phong, TenPhong, ISNULL(KhuVucNo,'') AS KhuVucNo, SucChua FROM PHONG WHERE ID_Phong=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                            return new
                            {
                                id = rd["ID_Phong"].ToString(),
                                ten = rd["TenPhong"].ToString(),
                                khuVucNo = rd["KhuVucNo"].ToString(),
                                sucChua = rd["SucChua"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SucChua"])
                            };
                }
            }
            return null;
        }

        public (bool ok, string msg) Create(string id, string ten, string khuVucNo, int? sucChua)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Vui lòng nhập ID phòng.");
            if (string.IsNullOrWhiteSpace(ten)) return (false, "Vui lòng nhập tên phòng.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM PHONG WHERE ID_Phong=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id.Trim()); if ((int)cmd.ExecuteScalar() > 0) return (false, "ID đã tồn tại."); }
                using (var cmd = new SqlCommand("INSERT INTO PHONG (ID_Phong,TenPhong,KhuVucNo,SucChua) VALUES (@id,@ten,@kv,@sc)", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id.Trim());
                    cmd.Parameters.AddWithValue("@ten", ten.Trim());
                    cmd.Parameters.AddWithValue("@kv", string.IsNullOrWhiteSpace(khuVucNo) ? (object)DBNull.Value : khuVucNo);
                    cmd.Parameters.AddWithValue("@sc", sucChua.HasValue ? (object)sucChua.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            return (true, "Tạo phòng thành công!");
        }

        public (bool ok, string msg) Update(string id, string ten, string khuVucNo, int? sucChua)
        {
            if (string.IsNullOrWhiteSpace(ten)) return (false, "Tên phòng không được để trống.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE PHONG SET TenPhong=@ten, KhuVucNo=@kv, SucChua=@sc WHERE ID_Phong=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@ten", ten.Trim());
                    cmd.Parameters.AddWithValue("@kv", string.IsNullOrWhiteSpace(khuVucNo) ? (object)DBNull.Value : khuVucNo);
                    cmd.Parameters.AddWithValue("@sc", sucChua.HasValue ? (object)sucChua.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            return (true, "Cập nhật thành công!");
        }

        public (bool ok, string msg) Delete(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                int soTB;
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM PHONG_THIETBI WHERE PhongNo=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id); soTB = (int)cmd.ExecuteScalar(); }
                if (soTB > 0) return (false, "Không thể xóa: phòng đang chứa " + soTB + " thiết bị.");
                using (var cmd = new SqlCommand("DELETE FROM PHONG WHERE ID_Phong=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            }
            return (true, "Xóa phòng thành công!");
        }
    }
}
