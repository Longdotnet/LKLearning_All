using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CCategory
    {
        
        public int CategoryId { get; set; }
        [Display(Name ="Tên danh mục")]

        public string CategoryName { get; set; } = null!;
        [Display(Name = "Mô tả")]

        public string? Description { get; set; }
    }
}
