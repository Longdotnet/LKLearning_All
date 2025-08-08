using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class ExamSet
    {
        public int ExamSetId { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? PassingScore { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? TimeLimitSec { get; set; }
    }
}
