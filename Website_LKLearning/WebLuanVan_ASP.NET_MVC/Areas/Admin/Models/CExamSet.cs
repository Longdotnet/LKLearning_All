using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CExamSet
    {
        public int ExamSetId { get; set; }
        [Display(Name="Mã Khóa học")]
        public int CourseId { get; set; }
        [Display(Name = "Tên khóa học")]
        public string? CourseName { get; set; }
        [Display(Name = "Tên bộ đề thi")]
        public string Name { get; set; } = null!;
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
        [Display(Name = "Điểm vượt qua")]
        public decimal? PassingScore { get; set; }
        [Display(Name="Ngày tạo")]
        public DateTime CreatedDate { get; set; }
        [Display(Name="Thời gian quy định")]
        public int? TimeLimitSec { get; set; }
    }
}
