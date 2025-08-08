using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int? LessonId { get; set; }
        public string? ReviewType { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public string? UrlAvatar { get; set; }



    }

}
