namespace WEBAPI.DTOS
{
    public class ExamSetQuestionDTO
    {
        public int ExamSetId { get; set; }

        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }

        public decimal QuestionScore { get; set; }

        public int? QuestionOrder { get; set; }




    }
}
