
    public class SwapLessonOrderDTO
    {
        public int CourseId { get; set; }   // khóa học
        public int SourceOrderNo { get; set; }   // vị trí hiện tại (ví dụ 1)
        public int TargetOrderNo { get; set; }   // vị trí muốn đổi sang (ví dụ 2)
    }
