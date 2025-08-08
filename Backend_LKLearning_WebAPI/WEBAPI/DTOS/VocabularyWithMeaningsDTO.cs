namespace WEBAPI.DTOS
{
    public class VocabularyWithMeaningsDTO
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; }
        public string Pronunciation { get; set; }
        public string AudioUrlUk { get; set; }
        public string AudioUrlUs { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<VocabularyMeaningDTO> Meanings { get; set; }
    }
}
