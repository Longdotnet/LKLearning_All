using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class LessonQuestion
{
    public int LessonId { get; set; }

    public int QuestionId { get; set; }

    public int OrderNo { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
