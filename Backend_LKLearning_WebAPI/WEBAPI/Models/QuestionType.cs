using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class QuestionType
{
    public int QuestionTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? TypeDescription { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
