using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class AcademicResult
{
    public int AcademicResultId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public int LessonId { get; set; }

    public string? Status { get; set; }

    public int? TimeSpent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
