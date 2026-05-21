using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QLTB.Models.Repositories
{
    public class KhoaRepository
    {
        public List<KhoaPhongBanViewModel> GetAll()
        {
            var list = new List<KhoaPhongBanViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT kp.ID_KhoaPhongBan, kp.TenPhongBanKhoa,
                           COUNT(DISTINCT nd.ID_NguoiDung) AS SoNguoiDung,
                           COUNT(DISTINCT tb.ID_ThietBi)   AS SoThietBi
                    FROM KHOA_PHONGBAN kp
                    LEFT JOIN NGUOIDUNG nd ON nd.Khoa_BanNo = kp.ID_KhoaPhongBan
                    LEFT JOIN THIETBI tb ON tb.KhoaPhongBan = kp.ID_KhoaPhongBan
                    GROUP BY kp.ID_KhoaPhongBan, kp.TenPhongBanKhoa
                    ORDER BY kp.TenPhongBanKhoa";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new KhoaPhongBanViewModel
                        {
                            ID_KhoaPhongBan = rd["ID_KhoaPhongBan"].ToString(),
                            TenPhongBanKhoa = rd["TenPhongBanKhoa"].ToString(),
                            SoNguoiDung = Convert.ToInt32(rd["SoNguoiDung"]),
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
                using (var cmd = new SqlCommand("SELECT ID_KhoaPhongBan, TenPhongBanKhoa FROM KHOA_PHONGBAN WHERE ID_KhoaPhongBan=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                            return new { id = rd["ID_KhoaPhongBan"].ToString(), ten = rd["TenPhongBanKhoa"].ToString() };
                }
            }
            return null;
        }

        public object GetChiTiet(string id)
        {
            var nguoiDung = new System.Collections.Generic.List<object>();
            var thietBi   = new System.Collections.Generic.List<object>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                // Danh sách người dùng
                const string sqlND = @"
                    SELECT nd.ID_NguoiDung, nd.HoTen, nd.Email,
                           ISNULL(vn.VaiTroNo,'') AS VaiTroNo,
                           CASE nd.TrangThaiTK WHEN 1 THEN N'Hoạt động' ELSE N'Khóa' END AS TrangThai
                    FROM NGUOIDUNG nd
                    LEFT JOIN VAITRO_NGUOIDUNG vn ON vn.NguoiDungNo = nd.ID_NguoiDung
                    WHERE nd.Khoa_BanNo = @id ORDER BY nd.HoTen";
                using (var cmd = new SqlCommand(sqlND, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            nguoiDung.Add(new {
                                id       = rd["ID_NguoiDung"].ToString(),
                                hoTen    = rd["HoTen"].ToString(),
                                email    = rd["Email"] == DBNull.Value ? "" : rd["Email"].ToString(),
                                vaiTro   = rd["VaiTroNo"].ToString(),
                                trangThai = rd["TrangThai"].ToString()
                            });
                }
                // Danh sách thiết bị
                const string sqlTB = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, tb.TrangThaiTB,
                           ISNULL(dm.TenDanhMuc,'') AS TenDanhMuc,
                           ISNULL(tb.Gia,0) AS Gia
                    FROM THIETBI tb
                    LEFT JOIN DANHMUC dm ON dm.ID_DanhMuc = tb.DanhMucNo
                    WHERE tb.KhoaPhongBan = @id ORDER BY tb.TenTB";
                using (var cmd = new SqlCommand(sqlTB, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            thietBi.Add(new {
                                id        = rd["ID_ThietBi"].ToString(),
                                tenTB     = rd["TenTB"].ToString(),
                                trangThai = rd["TrangThaiTB"]?.ToString() ?? "",
                                danhMuc   = rd["TenDanhMuc"].ToString(),
                                gia       = rd["Gia"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["Gia"])
                            });
                }
            }
            return new { nguoiDung, thietBi };
        }

        public (bool ok, string msg) Create(string id, string ten)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Vui lòng nhập ID khoa.");
            if (string.IsNullOrWhiteSpace(ten)) return (false, "Vui lòng nhập tên khoa.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM KHOA_PHONGBAN WHERE ID_KhoaPhongBan=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id.Trim()); if ((int)cmd.ExecuteScalar() > 0) return (false, "ID đã tồn tại."); }
                using (var cmd = new SqlCommand("INSERT INTO KHOA_PHONGBAN (ID_KhoaPhongBan,TenPhongBanKhoa) VALUES (@id,@ten)", conn))
                { cmd.Parameters.AddWithValue("@id", id.Trim()); cmd.Parameters.AddWithValue("@ten", ten.Trim()); cmd.ExecuteNonQuery(); }
            }
            return (true, "Tạo khoa thành công!");
        }

        public (bool ok, string msg) Update(string id, string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return (false, "Tên khoa không được để trống.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE KHOA_PHONGBAN SET TenPhongBanKhoa=@ten WHERE ID_KhoaPhongBan=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id); cmd.Parameters.AddWithValue("@ten", ten.Trim()); cmd.ExecuteNonQuery(); }
            }
            return (true, "Cập nhật thành công!");
        }

        public (bool ok, string msg) Delete(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                int soND, soTB;
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NGUOIDUNG WHERE Khoa_BanNo=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id); soND = (int)cmd.ExecuteScalar(); }
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM THIETBI WHERE KhoaPhongBan=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id); soTB = (int)cmd.ExecuteScalar(); }
                if (soND > 0 || soTB > 0)
                    return (false, "Không thể xóa: khoa đang có " + soND + " người dùng và " + soTB + " thiết bị.");
                using (var cmd = new SqlCommand("DELETE FROM KHOA_PHONGBAN WHERE ID_KhoaPhongBan=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            }
            return (true, "Xóa khoa thành công!");
        }

        public List<SelectListItem> GetDropdownList()
        {
            var list = new List<SelectListItem>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID_KhoaPhongBan, TenPhongBanKhoa FROM KHOA_PHONGBAN ORDER BY TenPhongBanKhoa", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new SelectListItem { Value = rd["ID_KhoaPhongBan"].ToString(), Text = rd["TenPhongBanKhoa"].ToString() });
            }
            return list;
        }
    }
}
