using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QLTB.Models.Repositories
{
    public class NguoiDungRepository
    {
        public List<NguoiDungViewModel> GetAll()
        {
            var list = new List<NguoiDungViewModel>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT nd.ID_NguoiDung, nd.HoTen, nd.Email, nd.TrangThaiTK,
                           ISNULL(kp.TenPhongBanKhoa,'') AS TenKhoa,
                           ISNULL(vn.VaiTroNo,'') AS VaiTroNo,
                           ISNULL(vt.TenVaiTro,'') AS TenVaiTro
                    FROM NGUOIDUNG nd
                    LEFT JOIN KHOA_PHONGBAN kp ON kp.ID_KhoaPhongBan = nd.Khoa_BanNo
                    LEFT JOIN VAITRO_NGUOIDUNG vn ON vn.NguoiDungNo = nd.ID_NguoiDung
                    LEFT JOIN VAITRO vt ON vt.ID_VaiTro = vn.VaiTroNo
                    ORDER BY nd.HoTen";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new NguoiDungViewModel
                        {
                            ID_NguoiDung = rd["ID_NguoiDung"].ToString(),
                            HoTen = rd["HoTen"].ToString(),
                            Email = rd["Email"]?.ToString() ?? "",
                            TrangThaiTK = Convert.ToBoolean(rd["TrangThaiTK"]),
                            TenKhoa = rd["TenKhoa"].ToString(),
                            VaiTroNo = rd["VaiTroNo"].ToString(),
                            TenVaiTro = rd["TenVaiTro"].ToString()
                        });
            }
            return list;
        }

        public object GetById(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT nd.ID_NguoiDung, nd.HoTen, nd.Email, nd.Khoa_BanNo, nd.TrangThaiTK,
                           ISNULL(vn.VaiTroNo,'') AS VaiTroNo
                    FROM NGUOIDUNG nd
                    LEFT JOIN VAITRO_NGUOIDUNG vn ON vn.NguoiDungNo = nd.ID_NguoiDung
                    WHERE nd.ID_NguoiDung = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                            return new
                            {
                                id = rd["ID_NguoiDung"].ToString(),
                                hoTen = rd["HoTen"].ToString(),
                                email = rd["Email"]?.ToString() ?? "",
                                khoaBanNo = rd["Khoa_BanNo"]?.ToString() ?? "",
                                trangThai = Convert.ToBoolean(rd["TrangThaiTK"]),
                                vaiTroNo = rd["VaiTroNo"].ToString()
                            };
                }
            }
            return null;
        }

        public (bool ok, string msg) Create(string id, string hoTen, string email, string matKhau, string khoaBanNo, string vaiTroNo, bool trangThai)
        {
            if (string.IsNullOrWhiteSpace(id)) return (false, "Vui lòng nhập ID.");
            if (string.IsNullOrWhiteSpace(hoTen)) return (false, "Họ tên không được để trống.");
            if (string.IsNullOrWhiteSpace(matKhau)) return (false, "Mật khẩu không được để trống.");

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NGUOIDUNG WHERE ID_NguoiDung=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id.Trim()); if ((int)cmd.ExecuteScalar() > 0) return (false, "ID đã tồn tại."); }

                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(@"INSERT INTO NGUOIDUNG (ID_NguoiDung,HoTen,Email,MatKhau,Khoa_BanNo,TrangThaiTK) VALUES (@id,@hoTen,@email,@mk,@khoa,@tt)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id.Trim());
                        cmd.Parameters.AddWithValue("@hoTen", hoTen.Trim());
                        cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@mk", matKhau);
                        cmd.Parameters.AddWithValue("@khoa", string.IsNullOrWhiteSpace(khoaBanNo) ? (object)DBNull.Value : khoaBanNo);
                        cmd.Parameters.AddWithValue("@tt", trangThai);
                        cmd.ExecuteNonQuery();
                    }
                    if (!string.IsNullOrWhiteSpace(vaiTroNo))
                        using (var cmd = new SqlCommand("INSERT INTO VAITRO_NGUOIDUNG (NguoiDungNo,VaiTroNo,NgayHieuLuc) VALUES (@nd,@vt,GETDATE())", conn, tran))
                        { cmd.Parameters.AddWithValue("@nd", id.Trim()); cmd.Parameters.AddWithValue("@vt", vaiTroNo); cmd.ExecuteNonQuery(); }
                    tran.Commit();
                }
            }
            return (true, "Tạo người dùng thành công!");
        }

        public (bool ok, string msg) Update(string id, string hoTen, string email, string matKhauMoi, string khoaBanNo, string vaiTroNo, bool trangThai)
        {
            if (string.IsNullOrWhiteSpace(hoTen)) return (false, "Họ tên không được để trống.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string sqlUpdate = string.IsNullOrWhiteSpace(matKhauMoi)
                        ? @"UPDATE NGUOIDUNG SET HoTen=@hoTen, Email=@email, Khoa_BanNo=@khoa, TrangThaiTK=@tt WHERE ID_NguoiDung=@id"
                        : @"UPDATE NGUOIDUNG SET HoTen=@hoTen, Email=@email, Khoa_BanNo=@khoa, TrangThaiTK=@tt, MatKhau=@mk WHERE ID_NguoiDung=@id";
                    using (var cmd = new SqlCommand(sqlUpdate, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@hoTen", hoTen.Trim());
                        cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@khoa", string.IsNullOrWhiteSpace(khoaBanNo) ? (object)DBNull.Value : khoaBanNo);
                        cmd.Parameters.AddWithValue("@tt", trangThai);
                        if (!string.IsNullOrWhiteSpace(matKhauMoi)) cmd.Parameters.AddWithValue("@mk", matKhauMoi);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("DELETE FROM VAITRO_NGUOIDUNG WHERE NguoiDungNo=@id", conn, tran))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    if (!string.IsNullOrWhiteSpace(vaiTroNo))
                        using (var cmd = new SqlCommand("INSERT INTO VAITRO_NGUOIDUNG (NguoiDungNo,VaiTroNo,NgayHieuLuc) VALUES (@nd,@vt,GETDATE())", conn, tran))
                        { cmd.Parameters.AddWithValue("@nd", id); cmd.Parameters.AddWithValue("@vt", vaiTroNo); cmd.ExecuteNonQuery(); }
                    tran.Commit();
                }
            }
            string msg = "Cập nhật thành công!" + (string.IsNullOrWhiteSpace(matKhauMoi) ? "" : " (đã đổi mật khẩu)");
            return (true, msg);
        }

        public (bool ok, string msg) Delete(string id, string currentUserId)
        {
            if (id == currentUserId) return (false, "Không thể xóa tài khoản đang đăng nhập.");
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand("DELETE FROM VAITRO_NGUOIDUNG WHERE NguoiDungNo=@id", conn, tran))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    using (var cmd = new SqlCommand("DELETE FROM NGUOIDUNG WHERE ID_NguoiDung=@id", conn, tran))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    tran.Commit();
                }
            }
            return (true, "Xóa người dùng thành công!");
        }

        public List<SelectListItem> GetVaiTroDropdown()
        {
            var list = new List<SelectListItem>();
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID_VaiTro, TenVaiTro FROM VAITRO ORDER BY TenVaiTro", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new SelectListItem { Value = rd["ID_VaiTro"].ToString(), Text = rd["TenVaiTro"].ToString() });
            }
            return list;
        }
    }
}
