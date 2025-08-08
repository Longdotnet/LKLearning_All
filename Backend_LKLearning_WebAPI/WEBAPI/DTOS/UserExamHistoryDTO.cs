namespace WEBAPI.DTOS
{
    public class UserExamHistoryDTO
    {
        public int HistoryId { get; set; }

        public int UserId { get; set; }
        public string? FullName { get; set; }

        public int ExamSetId { get; set; }

        public DateTime TakenAt { get; set; }

        public decimal? TotalScore { get; set; }

        public bool? IsPassed { get; set; }

        public int? DurationSec { get; set; }
    }
}
