namespace WEBAPI.DTOS
{
    public class QuestionDTO
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


    }
}
