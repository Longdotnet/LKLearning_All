using System.ComponentModel.DataAnnotations;
using Microsoft.Build.Framework;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;


namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CCourse
    {
        public int CourseId { get; set; }
        [Display(Name = "Tên khóa học")]
        [Required(ErrorMessage ="Tên khóa học không để trống")]
        public string CourseName { get; set; } = null!;
        [Display(Name = "Mô tả")]
   
        public string? Description { get; set; }
        [Display(Name = "Thời gian dự kiến học ")]
       
        public int DurationInMonths { get; set; }
        [Display(Name = "Cấp độ")]
       
        public int LevelId { get; set; }
        [Display(Name = "Danh mục")]
     
        public int CategoryId { get; set; }
        [Display(Name = "Gói cước")]
        
        public int? PackageId { get; set; }
        [Display(Name = "File Hình ảnh")]
     
        public string? UrlImage { get; set; }
        [Display(Name = "Thời hạn chứng chỉ")]
        public int? CertificateDurationDays { get; set; }
        public string CertificateDurationDaysView
        {
            get
            {
                return CertificateDurationDays.HasValue
                    ? CertificateDurationDays.Value.ToString()+ "ngày"
                    : "Vĩnh viễn";
            }
        }

    }
}
