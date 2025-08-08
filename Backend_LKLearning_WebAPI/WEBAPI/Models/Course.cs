using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationInMonths { get; set; }

    public int LevelId { get; set; }

    public int CategoryId { get; set; }

    public int? PackageId { get; set; }

    public string? Img { get; set; }

    public int? CertificateDurationDays { get; set; }

    public virtual ICollection<AcademicResult> AcademicResults { get; set; } = new List<AcademicResult>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<CourseLesson> CourseLessons { get; set; } = new List<CourseLesson>();

    public virtual ICollection<ExamSet> ExamSets { get; set; } = new List<ExamSet>();

    public virtual Level Level { get; set; } = null!;

    public virtual Package? Package { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
