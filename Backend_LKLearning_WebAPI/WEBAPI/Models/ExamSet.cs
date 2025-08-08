using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class ExamSet
{
    public int ExamSetId { get; set; }

    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? PassingScore { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? TimeLimitSec { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<ExamSetQuestion> ExamSetQuestions { get; set; } = new List<ExamSetQuestion>();

    public virtual ICollection<UserExamHistory> UserExamHistories { get; set; } = new List<UserExamHistory>();
}
