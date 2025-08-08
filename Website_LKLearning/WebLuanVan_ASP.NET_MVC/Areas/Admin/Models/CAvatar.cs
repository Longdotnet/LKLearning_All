using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CAvatar
    {

        public int AvatarId { get; set; }
        [Display(Name = "Link ảnh")]
        public string UrlPath { get; set; } = null!;
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
    }
}
