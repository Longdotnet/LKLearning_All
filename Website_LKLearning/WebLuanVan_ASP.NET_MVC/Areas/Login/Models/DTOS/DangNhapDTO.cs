using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Login.Models.DTOS
{
    public class DangNhapDTO
    {
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
        
        public DateOnly LastLoginUpdate { get; set; }
    }
}
