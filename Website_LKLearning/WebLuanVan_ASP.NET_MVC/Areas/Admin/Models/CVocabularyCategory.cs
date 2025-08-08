namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{

    public class CVocabularyCategory
    {
        public int VocabularyCategoryId { get; set; }

        public string VocabularyCategoryName { get; set; } = null!;

        public string? VocabularyCategoryDescription { get; set; }
        public string? UrlImage { get; set; }


        public DateTime? CreatedAt { get; set; }
    }
}
