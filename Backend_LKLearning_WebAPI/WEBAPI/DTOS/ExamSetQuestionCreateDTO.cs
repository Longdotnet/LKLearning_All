namespace WEBAPI.DTOS
{
    public class ExamSetQuestionCreateDTO
    {
        public int ExamSetId { get; set; }
        public int QuestionId { get; set; }
        public int QuestionScore { get; set; }
        public int QuestionOrder { get; set; }
    }

}
