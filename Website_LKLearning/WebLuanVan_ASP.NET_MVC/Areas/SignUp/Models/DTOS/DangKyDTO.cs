using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.SignUp.Models.DTOS
{
    
    public class DangKyDTO
    {
      
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;


    }
}
