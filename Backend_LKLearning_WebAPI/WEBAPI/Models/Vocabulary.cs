using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Vocabulary
{
    public int VocabularyId { get; set; }

    public string Word { get; set; } = null!;

    public string? Pronunciation { get; set; }

    public string? AudioUrlUk { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? AudioUrlUs { get; set; }

    public virtual ICollection<VocabularyCategoryMapping> VocabularyCategoryMappings { get; set; } = new List<VocabularyCategoryMapping>();

    public virtual ICollection<VocabularyMeaning> VocabularyMeanings { get; set; } = new List<VocabularyMeaning>();
}
