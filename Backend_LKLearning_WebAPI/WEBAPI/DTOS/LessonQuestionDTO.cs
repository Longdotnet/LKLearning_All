using WEBAPI.Models;

namespace WEBAPI.DTOS
{
    public class LessonQuestionDTO
    {
        public int LessonId { get; set; }

        public int QuestionId { get; set; }

        public int? OrderNo { get; set; }

    }
}
