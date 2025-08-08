using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CLevel
    {
        
        public int LevelId { get; set; }
        [Display(Name ="Tên cấp độ")]
        public string LevelName { get; set; } = null!;
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
    }
}
