using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CRole
    {
        [Display(Name = "Mã vai trò")]
        public int RoleId { get; set; }
        [Display(Name = "Tên vai trò")]
        public string? RoleName { get; set; }
    }
}
