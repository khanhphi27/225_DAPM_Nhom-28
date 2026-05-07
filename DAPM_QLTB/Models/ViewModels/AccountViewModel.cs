using System;
using System.ComponentModel.DataAnnotations;

namespace QLTB.Models
{
    // ── Thông báo ────────────────────────────────────────────
    public class ThongBaoViewModel
    {
        public string   ID_ThongBao  { get; set; }
        public string   TieuDe       { get; set; }
        public string   NoiDung      { get; set; }
        public DateTime NgayTao      { get; set; }
        public string   LoaiThongBao { get; set; }
        public bool     DaDoc        { get; set; }
        public string   NguoiTao     { get; set; }
    }

    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        // Vai trò: 1=Trưởng khoa, 2=Phòng CSVC, 3=Phòng KHTC, 4=BGH
        [Display(Name = "Vai trò")]
        public int RoleId { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
