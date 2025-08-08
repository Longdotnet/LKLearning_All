using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CPackage
    {
        public int PackageId { get; set; }
        [Display(Name="Tên gói")]
        public string PackageName { get; set; } = null!;
        [Display(Name ="Thời hạn")]
        public int? DurationDay { get; set; }
        [Display(Name = "Hình ảnh")]
        public string? UrlImage { get; set; }

        [Display(Name ="Giá")]
        public decimal? Price { get; set; }
        [Display(Name = "Sở hữu")]
        public List<int> IncludedPackageIds { get; set; } = new List<int>();

    }
}
