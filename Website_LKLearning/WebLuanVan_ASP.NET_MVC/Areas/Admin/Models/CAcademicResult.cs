namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CAcademicResult
    {
        public int AcademicResultId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int LessonId { get; set; }
        public string LessonTitle { get; set; }
        public string Status { get; set; }
        public int TimeSpent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
