namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CUserAnswer
    {
        public int UserAnswerId { get; set; }

        public int TestId { get; set; }

        public int QuestionId { get; set; }

        public int UserId { get; set; }

        public string SelectedAnswer { get; set; } = null!;

        public bool IsCorrect { get; set; }

        public DateTime AnsweredAt { get; set; }

    }
}
