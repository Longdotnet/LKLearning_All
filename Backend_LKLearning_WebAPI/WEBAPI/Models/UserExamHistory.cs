using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class UserExamHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public int ExamSetId { get; set; }

    public DateTime TakenAt { get; set; }

    public decimal? TotalScore { get; set; }

    public bool? IsPassed { get; set; }

    public int? DurationSec { get; set; }

    public virtual ExamSet ExamSet { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
