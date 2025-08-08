namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CVocabularyMeaning
    {
        public int VocabularyMeaningId { get; set; }

        public int VocabularyId { get; set; }

        public string Meaning { get; set; } = null!;

        public string? ExampleSentence { get; set; }

        public string? TranslatedMeaning { get; set; }

        public string? TranslatedExampleSentence { get; set; }

        public string? Synonyms { get; set; }

        public string? Antonyms { get; set; }

        public string? PartOfSpeech { get; set; }

    }
}