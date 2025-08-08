// Models/Question.cs
using System.Text.Json.Serialization;

namespace mauiluanvantotnghiep.Models;

public class Question
{
    public int QuestionId { get; set; }
    public int ContentTypeId { get; set; }
    public string? QuestionText { get; set; }
    public int QuestionTypeId { get; set; }  // 1 = MCQ, 2 = True/False, 3 = Fill in, 4 = Audio



    public decimal? QuestionScore { get; set; }
    public int? QuestionLevelId { get; set; }

    public string? QuestionDescription { get; set; }


    [JsonPropertyName("answerOptions")]
    public string? RawAnswerOptions { get; set; }

    public string? CorrectAnswer { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? Explanation { get; set; }

    public int? QuestionOrder { get; set; }

    [JsonIgnore]
    public string[] ParsedOptions { get; set; }

    [JsonIgnore]
    public string UserAnswer { get; set; }
}
