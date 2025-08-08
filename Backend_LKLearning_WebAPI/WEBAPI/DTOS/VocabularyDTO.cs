using System;
using System.Collections.Generic;

namespace WEBAPI.DTOS;

public class VocabularyDTO
{
    public int VocabularyId { get; set; }

    public string Word { get; set; } = null!;

    public string? Pronunciation { get; set; }

    public string? AudioUrlUk { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? AudioUrlUs { get; set; }
    public List<VocabularyMeaningDTO> Meanings { get; set; } = new List<VocabularyMeaningDTO>(); // Thêm
}

