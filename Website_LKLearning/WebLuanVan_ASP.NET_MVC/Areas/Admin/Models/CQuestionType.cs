using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CQuestionType
    {
        public int QuestionTypeId { get; set; }
        [Display(Name="Tên loại")]
        public string TypeName { get; set; } = null!;
        [Display(Name ="Mô tả")]
        public string? TypeDescription { get; set; }
    }
}
