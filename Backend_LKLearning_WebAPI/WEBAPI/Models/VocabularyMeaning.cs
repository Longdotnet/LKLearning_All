using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class VocabularyMeaning
{
    public int VocabularyMeaningId { get; set; }

    public int VocabularyId { get; set; }

    public string? Meaning { get; set; }

    public string? ExampleSentence { get; set; }

    public string? TranslatedMeaning { get; set; }

    public string? TranslatedExampleSentence { get; set; }

    public string? Synonyms { get; set; }

    public string? Antonyms { get; set; }

    public string? PartOfSpeech { get; set; }

    public virtual Vocabulary Vocabulary { get; set; } = null!;
}
