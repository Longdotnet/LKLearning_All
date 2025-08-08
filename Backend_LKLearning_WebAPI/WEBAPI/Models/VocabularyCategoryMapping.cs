using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class VocabularyCategoryMapping
{
    public int VocabularyId { get; set; }

    public int VocabularyCategoryId { get; set; }

    public DateTime? DateAdded { get; set; }

    public virtual Vocabulary Vocabulary { get; set; } = null!;

    public virtual VocabularyCategory VocabularyCategory { get; set; } = null!;
}
