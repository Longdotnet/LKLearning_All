namespace WEBAPI.DTOS
{
    public class VocabularyCategoryDTO
    {
        public int VocabularyCategoryId { get; set; }

        public string VocabularyCategoryName { get; set; } = null!;

        public string? VocabularyCategoryDescription { get; set; }
        public string? UrlImage { get; set; }


        public DateTime? CreatedAt { get; set; }
        public int? VocabularyCount { get; set; }
    }
}
