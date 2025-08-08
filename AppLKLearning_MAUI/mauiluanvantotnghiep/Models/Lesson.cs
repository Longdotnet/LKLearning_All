using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class Lesson
    {
        public int LessonId { get; set; }
        public string? LessonTitle { get; set; }
        public string? LessonContent { get; set; }
        public string? LessonDescription { get; set; }
        public DateOnly Duration { get; set; }

        public int DurationMinute { get; set; }
        public bool IsActivate { get; set; }

        public string? UrlImageLesson { get; set; }

        // Thêm OrderNo để hiển thị thứ tự bài học
        public int? OrderNo { get; set; }

        // Thêm vào
        // Thuộc tính kiểu Color
        public Color? RowColor { get; set; }


        // Mới: status text để bind
        public string? StatusText { get; set; } = "Start";
    }

}
