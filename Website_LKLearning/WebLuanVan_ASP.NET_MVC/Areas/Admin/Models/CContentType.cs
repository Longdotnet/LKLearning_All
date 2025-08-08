using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CContentType
    {
        public int ContentTypeId { get; set; }
        [Display(Name ="Tên chủ đề")]
        public string TypeName { get; set; } = null!;
        [Display(Name ="Mô tả")]
        public string? TypeDescription { get; set; }
    }
}
