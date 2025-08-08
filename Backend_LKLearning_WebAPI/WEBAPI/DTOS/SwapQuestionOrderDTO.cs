namespace WEBAPI.DTOS
{
    public class SwapQuestionOrderDTO
    {
        public int LessonId { get; set; }   // Bài học
        public int SourceOrderNo { get; set; }   // Vị trí hiện tại (ví dụ 1)
        public int TargetOrderNo { get; set; }   // Vị trí muốn đổi (ví dụ 3)

    }
}
