using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace QLTB.Models.Repositories
{
    public class ThietBiRepository
    {
        #region ── Dropdown helpers ─────────────────────────────

        public List<SelectListItem> LoadDropdown(SqlConnection conn, string sql)
        {
            var items = new List<SelectListItem>();
            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
                while (rd.Read())
                    items.Add(new SelectListItem { Value = rd["Value"].ToString(), Text = rd["Text"].ToString() });
            return items;
        }

        public List<KhuViewModel> GetKhuVucList(SqlConnection conn)
        {
            var list = new List<KhuViewModel>();
            const string sql = @"SELECT kv.ID_KhuVuc, kv.TenKhuVuc, ISNULL(kv.CoSoNo,'') AS CoSoNo, ISNULL(cs.TenCoSo,'') AS TenCoSo
                                 FROM KHUVUC kv LEFT JOIN COSO cs ON cs.ID_CoSo=kv.CoSoNo ORDER BY cs.TenCoSo, kv.TenKhuVuc";
            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
                while (rd.Read())
                    list.Add(new KhuViewModel { ID_KhuVuc = rd["ID_KhuVuc"].ToString(), TenKhuVuc = rd["TenKhuVuc"].ToString(), CoSoNo = rd["CoSoNo"].ToString(), TenCoSo = rd["TenCoSo"].ToString() });
            return list;
        }

        public List<PhongViewModel> GetPhongList(SqlConnection conn)
        {
            var list = new List<PhongViewModel>();
            const string sql = @"SELECT p.ID_Phong, p.TenPhong, ISNULL(kv.CoSoNo,'') AS CoSoNo, ISNULL(p.KhuVucNo,'') AS KhuVucNo, ISNULL(kv.TenKhuVuc,'') AS TenKhuVuc
                                 FROM PHONG p LEFT JOIN KHUVUC kv ON kv.ID_KhuVuc=p.KhuVucNo ORDER BY kv.TenKhuVuc, p.TenPhong";
            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
                while (rd.Read())
                    list.Add(new PhongViewModel { ID_Phong = rd["ID_Phong"].ToString(), TenPhong = rd["TenPhong"].ToString(), CoSoNo = rd["CoSoNo"].ToString(), KhuVucNo = rd["KhuVucNo"].ToString(), TenKhuVuc = rd["TenKhuVuc"].ToString() });
            return list;
        }

        #endregion

        #region ── Danh sách thiết bị ──────────────────────────

        /// <summary>Danh sách thiết bị + dữ liệu dropdown cho trang QuanLyThietBi (CSVC).</summary>
        public (List<ThietBiViewModel> list, List<SelectListItem> danhMuc, List<SelectListItem> khoa,
                List<SelectListItem> coSo, List<KhuViewModel> khu, List<SelectListItem> ncc, List<PhongViewModel> phong)
            GetAllForManagement()
        {
            var list = new List<ThietBiViewModel>();
            List<SelectListItem> danhMuc, khoa, coSo, ncc;
            List<KhuViewModel> khu;
            List<PhongViewModel> phong;

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                danhMuc = LoadDropdown(conn, "SELECT DISTINCT ID_DanhMuc AS Value, TenDanhMuc AS Text FROM DANHMUC ORDER BY TenDanhMuc");
                khoa = LoadDropdown(conn, "SELECT DISTINCT ID_KhoaPhongBan AS Value, TenPhongBanKhoa AS Text FROM KHOA_PHONGBAN ORDER BY TenPhongBanKhoa");
                coSo = LoadDropdown(conn, "SELECT DISTINCT ID_CoSo AS Value, TenCoSo AS Text FROM COSO ORDER BY TenCoSo");
                khu = GetKhuVucList(conn);
                ncc = LoadDropdown(conn, "SELECT DISTINCT ID_NhaCC AS Value, TenNhaCC AS Text FROM NHACUNGCAP ORDER BY TenNhaCC");
                phong = GetPhongList(conn);

                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, dm.TenDanhMuc, kp.TenPhongBanKhoa, ncc.TenNhaCC,
                           tb.SoSeri, tb.Gia, tb.TrangThaiTB, tb.DeXuatNo, ptb.PhongNo,
                           ISNULL(p.KhuVucNo,'') AS PhongKhuVucNo, ISNULL(kv.CoSoNo,'') AS PhongCoSoNo
                    FROM THIETBI tb
                    LEFT JOIN DANHMUC dm ON tb.DanhMucNo=dm.ID_DanhMuc
                    LEFT JOIN KHOA_PHONGBAN kp ON tb.KhoaPhongBan=kp.ID_KhoaPhongBan
                    LEFT JOIN NHACUNGCAP ncc ON tb.NhaCCNo=ncc.ID_NhaCC
                    OUTER APPLY (SELECT TOP 1 ptb.PhongNo FROM PHONG_THIETBI ptb WHERE ptb.ThietBiNo=tb.ID_ThietBi ORDER BY ISNULL(ptb.NgayHieuLuc,'19000101') DESC) ptb
                    LEFT JOIN PHONG p ON p.ID_Phong=ptb.PhongNo
                    LEFT JOIN KHUVUC kv ON kv.ID_KhuVuc=p.KhuVucNo";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(MapThietBiFromReader(rd, includeNames: true));
            }
            return (list, danhMuc, khoa, coSo, khu, ncc, phong);
        }

        /// <summary>Chi tiết 1 thiết bị (cho trang ChiTietThietBi).</summary>
        public ThietBiViewModel GetDetail(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, tb.DanhMucNo, tb.KhoaPhongBan, tb.NhaCCNo, tb.ThongSoKT,
                           dm.TenDanhMuc, kp.TenPhongBanKhoa, ncc.TenNhaCC,
                           tb.SoSeri, tb.Gia, tb.TrangThaiTB, tb.DeXuatNo,
                           ptb.PhongNo, ISNULL(p.TenPhong,'') AS TenPhong,
                           ISNULL(p.KhuVucNo,'') AS PhongKhuVucNo, ISNULL(kv.TenKhuVuc,'') AS TenKhuVuc,
                           ISNULL(kv.CoSoNo,'') AS PhongCoSoNo, ISNULL(cs.TenCoSo,'') AS TenCoSo
                    FROM THIETBI tb
                    LEFT JOIN DANHMUC dm ON tb.DanhMucNo=dm.ID_DanhMuc
                    LEFT JOIN KHOA_PHONGBAN kp ON tb.KhoaPhongBan=kp.ID_KhoaPhongBan
                    LEFT JOIN NHACUNGCAP ncc ON tb.NhaCCNo=ncc.ID_NhaCC
                    OUTER APPLY (SELECT TOP 1 ptb.PhongNo FROM PHONG_THIETBI ptb WHERE ptb.ThietBiNo=tb.ID_ThietBi ORDER BY ISNULL(ptb.NgayHieuLuc,'19000101') DESC) ptb
                    LEFT JOIN PHONG p ON p.ID_Phong=ptb.PhongNo
                    LEFT JOIN KHUVUC kv ON kv.ID_KhuVuc=p.KhuVucNo
                    LEFT JOIN COSO cs ON cs.ID_CoSo=kv.CoSoNo
                    WHERE tb.ID_ThietBi=@id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                            return new ThietBiViewModel
                            {
                                ID_ThietBi = rd["ID_ThietBi"].ToString(), TenTB = rd["TenTB"].ToString(),
                                DanhMuc = rd["DanhMucNo"]?.ToString() ?? "", TenDanhMuc = rd["TenDanhMuc"]?.ToString() ?? "",
                                KhoaPhongBan = rd["KhoaPhongBan"]?.ToString() ?? "", TenKhoaPhongBan = rd["TenPhongBanKhoa"]?.ToString() ?? "",
                                PhongCoSoNo = rd["PhongCoSoNo"] == DBNull.Value ? "" : rd["PhongCoSoNo"].ToString(),
                                TenCoSo = rd["TenCoSo"]?.ToString() ?? "",
                                PhongKhuVucNo = rd["PhongKhuVucNo"] == DBNull.Value ? "" : rd["PhongKhuVucNo"].ToString(),
                                TenKhuVuc = rd["TenKhuVuc"]?.ToString() ?? "",
                                PhongNo = rd["PhongNo"] == DBNull.Value ? "" : rd["PhongNo"].ToString(),
                                TenPhong = rd["TenPhong"]?.ToString() ?? "",
                                NhaCungCap = rd["NhaCCNo"]?.ToString() ?? "", TenNhaCungCap = rd["TenNhaCC"]?.ToString() ?? "",
                                SoSeri = rd["SoSeri"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SoSeri"]),
                                Gia = rd["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["Gia"]),
                                TrangThaiTB = rd["TrangThaiTB"]?.ToString() ?? "",
                                DeXuatNo = rd["DeXuatNo"] == DBNull.Value ? "" : rd["DeXuatNo"].ToString(),
                                ThongSoKT = rd["ThongSoKT"]?.ToString() ?? ""
                            };
                }
            }
            return null;
        }

        /// <summary>Lấy thiết bị theo ID (JSON response cho SaveThietBi modal).</summary>
        public object GetById(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                const string sql = @"
                    SELECT tb.ID_ThietBi, tb.TenTB, tb.DanhMucNo, tb.KhoaPhongBan, tb.NhaCCNo,
                           tb.SoSeri, tb.Gia, tb.TrangThaiTB, tb.DeXuatNo, ptb.PhongNo,
                           ISNULL(p.KhuVucNo,'') AS PhongKhuVucNo, ISNULL(kv.CoSoNo,'') AS PhongCoSoNo
                    FROM THIETBI tb
                    OUTER APPLY (SELECT TOP 1 ptb.PhongNo FROM PHONG_THIETBI ptb WHERE ptb.ThietBiNo=tb.ID_ThietBi ORDER BY ISNULL(ptb.NgayHieuLuc,'19000101') DESC) ptb
                    LEFT JOIN PHONG p ON p.ID_Phong=ptb.PhongNo
                    LEFT JOIN KHUVUC kv ON kv.ID_KhuVuc=p.KhuVucNo
                    WHERE tb.ID_ThietBi=@id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                            return new
                            {
                                ID_ThietBi = rd["ID_ThietBi"].ToString(), TenTB = rd["TenTB"].ToString(),
                                DanhMuc = rd["DanhMucNo"]?.ToString() ?? "", KhoaPhongBan = rd["KhoaPhongBan"]?.ToString() ?? "",
                                PhongCoSoNo = rd["PhongCoSoNo"] == DBNull.Value ? "" : rd["PhongCoSoNo"].ToString(),
                                PhongKhuVucNo = rd["PhongKhuVucNo"] == DBNull.Value ? "" : rd["PhongKhuVucNo"].ToString(),
                                NhaCungCap = rd["NhaCCNo"]?.ToString() ?? "",
                                SoSeri = rd["SoSeri"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SoSeri"]),
                                Gia = rd["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["Gia"]),
                                TrangThaiTB = rd["TrangThaiTB"]?.ToString() ?? "",
                                DeXuatNo = rd["DeXuatNo"] == DBNull.Value ? "" : rd["DeXuatNo"].ToString()
                            };
                }
            }
            return null;
        }

        #endregion

        #region ── CRUD ────────────────────────────────────────

        public (bool ok, string msg) Save(ThietBiViewModel tb)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                bool isUpdate = !string.IsNullOrWhiteSpace(tb.ID_ThietBi);
                if (!isUpdate) tb.ID_ThietBi = GenerateNewId(conn, tb.NhaCungCap, tb.KhoaPhongBan, tb.DanhMuc);
                else if (!IdExists(conn, tb.ID_ThietBi)) return (false, "Không tìm thấy thiết bị để sửa");

                var deXuatNo = string.IsNullOrWhiteSpace(tb.DeXuatNo) ? null : tb.DeXuatNo.Trim();
                if (!string.IsNullOrEmpty(deXuatNo) && !IsDeXuatApprovedByBGH(conn, deXuatNo))
                    return (false, "Mã đề xuất không hợp lệ. Chỉ chấp nhận đề xuất đã duyệt cuối bởi BGH.");

                var phongNo = string.IsNullOrWhiteSpace(tb.PhongNo) ? null : tb.PhongNo.Trim();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        string sql = isUpdate
                            ? @"UPDATE THIETBI SET TenTB=@ten, DanhMucNo=@dm, KhoaPhongBan=@khoa, NhaCCNo=@ncc, SoSeri=@seri, Gia=@gia, TrangThaiTB=@tt, DeXuatNo=@dx WHERE ID_ThietBi=@id"
                            : @"INSERT INTO THIETBI (ID_ThietBi,TenTB,DanhMucNo,KhoaPhongBan,NhaCCNo,SoSeri,Gia,TrangThaiTB,DeXuatNo) VALUES (@id,@ten,@dm,@khoa,@ncc,@seri,@gia,@tt,@dx)";
                        using (var cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", tb.ID_ThietBi);
                            cmd.Parameters.AddWithValue("@ten", tb.TenTB ?? "");
                            cmd.Parameters.AddWithValue("@dm", (object)tb.DanhMuc ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@khoa", (object)tb.KhoaPhongBan ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ncc", (object)tb.NhaCungCap ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@seri", (object)tb.SoSeri ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@gia", (object)tb.Gia ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@tt", tb.TrangThaiTB ?? "Mới nhập");
                            cmd.Parameters.AddWithValue("@dx", (object)deXuatNo ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand("DELETE FROM PHONG_THIETBI WHERE ThietBiNo=@id", conn, tran))
                        { cmd.Parameters.AddWithValue("@id", tb.ID_ThietBi); cmd.ExecuteNonQuery(); }
                        if (!string.IsNullOrWhiteSpace(phongNo))
                            using (var cmd = new SqlCommand("INSERT INTO PHONG_THIETBI (ThietBiNo,PhongNo,NgayHieuLuc) VALUES (@tb,@phong,GETDATE())", conn, tran))
                            { cmd.Parameters.AddWithValue("@tb", tb.ID_ThietBi); cmd.Parameters.AddWithValue("@phong", phongNo); cmd.ExecuteNonQuery(); }
                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
            return (true, "Lưu thiết bị thành công!");
        }

        public (bool ok, string msg) Delete(string id)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM THIETBI WHERE ID_ThietBi=@id", conn))
                { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            }
            return (true, "Xóa thiết bị thành công!");
        }

        public string GenerateNewId(SqlConnection conn, string nhaCCNo, string khoaNo, string danhMucNo)
        {
            // Format 9 ký tự: YY(2) + Khoa(2) + DanhMuc(2) + STT(3)
            string yy       = DateTime.Now.ToString("yy");
            string khoaPart = ExtractNumericSuffix(khoaNo,    2);
            string dmPart   = ExtractNumericSuffix(danhMucNo, 2);
            string prefix   = yy + khoaPart + dmPart; // 6 ký tự

            int count = 0;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM THIETBI WHERE ID_ThietBi LIKE @p + '%'", conn))
            {
                cmd.Parameters.AddWithValue("@p", prefix);
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return prefix + (count + 1).ToString("D3"); // tổng = 9 ký tự
        }

        /// <summary>
        /// Lấy 2 ký tự cuối cùng của chuỗi ID (bất kể chữ hay số).
        /// Ví dụ: "KP02" → "02" | "DM_MT" → "MT" | "K1" → "K1" | "A" → "0A"
        /// </summary>
        private string ExtractNumericSuffix(string id, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(id)) return new string('0', maxLen);
            var s = id.Trim().ToUpper();
            return s.Length >= maxLen
                ? s.Substring(s.Length - maxLen)   // lấy maxLen ký tự cuối
                : s.PadLeft(maxLen, '0');           // nếu ngắn hơn thì pad trái bằng '0'
        }

        #endregion

        #region ── Private helpers ─────────────────────────────

        private bool IdExists(SqlConnection conn, string id)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM THIETBI WHERE ID_ThietBi=@id", conn))
            { cmd.Parameters.AddWithValue("@id", id); return (int)cmd.ExecuteScalar() > 0; }
        }

        private bool IsDeXuatApprovedByBGH(SqlConnection conn, string deXuatNo)
        {
            const string sql = @"
                SELECT COUNT(1) FROM DEXUAT_MUASAM dx
                WHERE dx.ID_DeXuat=@dx AND LTRIM(RTRIM(dx.TrangThai))=N'Đã duyệt'
                  AND EXISTS (SELECT 1 FROM LICHSUDUYET ls JOIN VAITRO_NGUOIDUNG vn ON vn.NguoiDungNo=ls.NguoiDuyetNo AND vn.VaiTroNo=N'VT_BGH'
                              WHERE ls.DeXuatNo=dx.ID_DeXuat AND ls.ThoiGianDuyet=(SELECT MAX(ls2.ThoiGianDuyet) FROM LICHSUDUYET ls2 WHERE ls2.DeXuatNo=dx.ID_DeXuat)
                              AND ls.CapDuyet=4 AND LTRIM(RTRIM(ls.TrangThaiSauDuyet))=N'Đã duyệt')";
            using (var cmd = new SqlCommand(sql, conn))
            { cmd.Parameters.AddWithValue("@dx", deXuatNo); return Convert.ToInt32(cmd.ExecuteScalar()) > 0; }
        }

        private ThietBiViewModel MapThietBiFromReader(SqlDataReader rd, bool includeNames)
        {
            return new ThietBiViewModel
            {
                ID_ThietBi = rd["ID_ThietBi"].ToString(), TenTB = rd["TenTB"].ToString(),
                DanhMuc = rd["TenDanhMuc"]?.ToString() ?? "", TenDanhMuc = rd["TenDanhMuc"]?.ToString() ?? "",
                KhoaPhongBan = rd["TenPhongBanKhoa"]?.ToString() ?? "", TenKhoaPhongBan = rd["TenPhongBanKhoa"]?.ToString() ?? "",
                PhongCoSoNo = rd["PhongCoSoNo"] == DBNull.Value ? "" : rd["PhongCoSoNo"].ToString(),
                PhongKhuVucNo = rd["PhongKhuVucNo"] == DBNull.Value ? "" : rd["PhongKhuVucNo"].ToString(),
                PhongNo = rd["PhongNo"] == DBNull.Value ? "" : rd["PhongNo"].ToString(),
                NhaCungCap = rd["TenNhaCC"]?.ToString() ?? "", TenNhaCungCap = rd["TenNhaCC"]?.ToString() ?? "",
                SoSeri = rd["SoSeri"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["SoSeri"]),
                Gia = rd["Gia"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["Gia"]),
                TrangThaiTB = rd["TrangThaiTB"]?.ToString() ?? "",
                DeXuatNo = rd["DeXuatNo"] == DBNull.Value ? "" : rd["DeXuatNo"].ToString()
            };
        }

        #endregion
    }
}
