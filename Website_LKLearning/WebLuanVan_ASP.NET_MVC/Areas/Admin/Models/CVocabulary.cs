namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CVocabulary
    {
        public int VocabularyId { get; set; }

        public string Word { get; set; } = null!;

        public string? Pronunciation { get; set; }

        public string? AudioUrlUk { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? AudioUrlUs { get; set; }
        public List<CVocabularyMeaning> Meanings { get; set; } = new List<CVocabularyMeaning>(); // Thêm
    }

}