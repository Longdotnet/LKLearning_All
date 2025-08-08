using System;
using System.Collections.Generic;

namespace WEBAPI.DTOS;

public class ReviewDTO
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }
    public string? CourseName { get; set; }

    public int? LessonId { get; set; }
    public string? LessonName { get; set; }

    public string ReviewType { get; set; } = null!;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }
    public string? Name { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UrlAvatar { get; set; }


}
