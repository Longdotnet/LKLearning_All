using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int? ContentTypeId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int? QuestionTypeId { get; set; }

    public string? AnswerOptions { get; set; }

    public string? CorrectAnswer { get; set; }

    public string? ImageUrl { get; set; }

    public string? AudioUrl { get; set; }

    public string? Explanation { get; set; }

    public int? QuestionLevelId { get; set; }

    public string? QuestionDescription { get; set; }

    public virtual ContentType? ContentType { get; set; }

    public virtual ICollection<ExamSetQuestion> ExamSetQuestions { get; set; } = new List<ExamSetQuestion>();

    public virtual ICollection<LessonQuestion> LessonQuestions { get; set; } = new List<LessonQuestion>();

    public virtual QuestionLevel? QuestionLevel { get; set; }

    public virtual QuestionType? QuestionType { get; set; }
}
