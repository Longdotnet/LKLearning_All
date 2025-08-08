using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class ExamSetQuestion
{
    public int ExamSetId { get; set; }

    public int QuestionId { get; set; }

    public decimal QuestionScore { get; set; }

    public int? QuestionOrder { get; set; }

    public virtual ExamSet ExamSet { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
