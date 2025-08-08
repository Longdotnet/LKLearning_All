using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public int? LessonId { get; set; }

    public string ReviewType { get; set; } = null!;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Lesson? Lesson { get; set; }

    public virtual User User { get; set; } = null!;
}
