using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class VocabularyCategory
{
    public int VocabularyCategoryId { get; set; }

    public string VocabularyCategoryName { get; set; } = null!;

    public string? VocabularyCategoryDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UrlImage { get; set; }

    public virtual ICollection<VocabularyCategoryMapping> VocabularyCategoryMappings { get; set; } = new List<VocabularyCategoryMapping>();
}
