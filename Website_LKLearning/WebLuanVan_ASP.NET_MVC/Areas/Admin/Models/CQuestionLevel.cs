using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models

{
    public class CQuestionLevel
    {
        public int QuestionLevelId { get; set; }
        [Display(Name = "Tên cấp độ câu hỏi")]
        public string? QuestionName { get; set; }
    }
}
