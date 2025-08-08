using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CLesson
    {
        public int LessonId { get; set; }
        [Display(Name = " Tên bài học")]
        [Required(ErrorMessage ="Tên bài học không để trống")]
        public string LessonTitle { get; set; } = null!;
        [Display(Name = "Nội dung bài học")]
        public string? LessonContent { get; set; }
        [Display(Name = "Mô tả")]
        public string? LessonDescription { get; set; }
        [Display(Name = "Hoàn thành")]
        public DateOnly Duration { get; set; }
        [Display(Name = "Kích hoạt")]
        public bool IsActivate { get; set; }

        [Display(Name = "Thời gian làm bài")]
        public int DurationMinute { get; set; }
        [Display(Name ="Ảnh bài học")]
        public string? UrlImageLesson { get; set; }

    }
}
