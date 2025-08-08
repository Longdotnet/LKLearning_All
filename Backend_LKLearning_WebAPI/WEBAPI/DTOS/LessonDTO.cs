using System.ComponentModel.DataAnnotations;

namespace WEBAPI.DTOS
{
    public class LessonDTO
    {
        [Display(Name ="ID bài học")]
        public int LessonId { get; set; }
      
        [Display(Name ="Tên bài học")]
        public string LessonTitle { get; set; } = null!;
        [Display(Name ="Nội dung bài học")]
        public string? LessonContent { get; set; }
        [Display(Name ="Mô tả bài học")]
        public string? LessonDescription { get; set; }
        [Display(Name ="Thời gian tạo bài học")]
        public DateOnly Duration { get; set; }

        [Display(Name = "Thời gian làm bài")]
        public int DurationMinute { get; set; }

        [Display(Name ="Kích hoạt")]
        public bool IsActivate { get; set; }

        public string? UrlImageLesson { get; set; }
    }
}
