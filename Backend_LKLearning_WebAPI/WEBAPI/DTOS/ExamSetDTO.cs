namespace WEBAPI.DTOS
{
    public class ExamSetDTO
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
