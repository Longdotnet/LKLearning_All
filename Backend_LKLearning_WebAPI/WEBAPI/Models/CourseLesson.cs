using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class CourseLesson
{
    public int CourseId { get; set; }

    public int LessonId { get; set; }

    public int OrderNo { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
