using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class QuestionLevel
{
    public int QuestionLevelId { get; set; }

    public string? QuestionName { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
