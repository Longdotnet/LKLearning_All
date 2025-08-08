using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Lesson
{
    public int LessonId { get; set; }

    public string LessonTitle { get; set; } = null!;

    public string? LessonContent { get; set; }

    public string? LessonDescription { get; set; }

    public DateOnly Duration { get; set; }

    public int DurationMinute { get; set; }

    public bool IsActivate { get; set; }

    public string? UrlImageLesson { get; set; }

    public virtual ICollection<AcademicResult> AcademicResults { get; set; } = new List<AcademicResult>();

    public virtual ICollection<CourseLesson> CourseLessons { get; set; } = new List<CourseLesson>();

    public virtual ICollection<LessonQuestion> LessonQuestions { get; set; } = new List<LessonQuestion>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
