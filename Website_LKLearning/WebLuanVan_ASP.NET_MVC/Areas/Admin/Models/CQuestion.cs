using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IdentityModel.Tokens.Jwt;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CQuestion
    {
        [Display(Name ="ID câu hỏi")]
        public int QuestionId { get; set; }
        [Display(Name = "Thuộc loại")]

        public int? ContentTypeId { get; set; }
        [Display(Name = "Tên câu hỏi")]
        public string QuestionText { get; set; } = null!;
        [Display(Name = "Thuộc loại")]
        public int? QuestionTypeId { get; set; }
        [Display(Name = "Mức độ câu hỏi")]
        public int? QuestionLevelId { get; set; }
        [Display(Name = "Lựa chọn đáp án")]
        // Thuộc tính sẽ lưu dưới dạng chuỗi JSON trong cơ sở dữ liệu
        public string? AnswerOptions { get; set; }

        // Thuộc tính phụ phục vụ cho binding (không map vào DB)
        //giúp bạn làm việc với dữ liệu dưới dạng danh sách khi binding trong view.Khi gán,
        //nó tự động serializes danh sách thành chuỗi JSON để lưu vào AnswerOptions.
        [NotMapped]
        [Display(Name ="Danh sách lựa chọn")]
        public List<string> AnswerOptionsList
        {
            get
            {
                if (string.IsNullOrEmpty(AnswerOptions))
                    return new List<string>();
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(AnswerOptions);
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
                AnswerOptions = JsonConvert.SerializeObject(value);
            }
        }
        [NotMapped]
        [Display(Name = "Nội dung transcript")]
        public string? TranscriptText { get; set; }
        [Display(Name = "Đáp án đúng")]
       
        public string? CorrectAnswer { get; set; }
        [Display(Name = "Link hình ảnh")]
        public string? ImageUrl { get; set; }
        [Display(Name ="Link âm thanh")]
        public string? AudioUrl { get; set; }
        [Display(Name = "Giải thích")]
        public string? Explanation { get; set; }
        public virtual ICollection<CLessonQuestion> LessonQuestions { get; set; }
        public string? QuestionDescription { get; set; }

    }
}
