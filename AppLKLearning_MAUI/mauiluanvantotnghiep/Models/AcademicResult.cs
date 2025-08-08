using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class AcademicResult
    {
        public int AcademicResultId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int LessonId { get; set; }
        public string Status { get; set; }    // "InProgress" / "Completed"
                                              // … các trường khác nếu cần
    }
}
