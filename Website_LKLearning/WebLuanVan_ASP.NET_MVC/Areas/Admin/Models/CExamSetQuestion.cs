using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CExamSetQuestion
    {
        public int ExamSetId { get; set; }

        public int QuestionId { get; set; }
        [Display(Name = "Câu hỏi")]
        public string? QuestionText { get; set; }
        [Display(Name = "Điểm câu hỏi")]
        public decimal QuestionScore { get; set; }
        [Display(Name = "Thứ tự câu hỏi")]
        public int? QuestionOrder { get; set; }
    }
}
