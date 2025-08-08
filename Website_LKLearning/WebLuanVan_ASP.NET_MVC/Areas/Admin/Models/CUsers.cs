using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CUsers
    {
        [Display(Name ="ID người dùng")]
        public int UserId { get; set; }
        [Display(Name = "Tên tài khoản")]

        public string? Fullname { get; set; }

        public string? Email { get; set; }
        [Display(Name ="Mật khẩu")]
        public string? Password { get; set; }
        [Display(Name = "Tuổi")]
        public int? Age { get; set; }
        [Display(Name =" Số điện thoại")]
        public string? Phone { get; set; }
        [Display(Name ="Giới tính")]
        public bool? Gender { get; set; }
        [Display(Name="Sinh nhật")]
        public DateOnly? DateofBirth { get; set; }
        [Display(Name="Đăng nhập gần đây")]
        public DateTime? LastLoginDate { get; set; }
        [Display(Name ="Mã vai trò")]
        public int? RoleId { get; set; }
        [Display(Name = "Mã người dùng")]
        public string DisplayUserId => $"HV{UserId:0000}";
        [Display(Name = "Avatar")]
        public string? AvatarUrl { get; set; }

    }
}
