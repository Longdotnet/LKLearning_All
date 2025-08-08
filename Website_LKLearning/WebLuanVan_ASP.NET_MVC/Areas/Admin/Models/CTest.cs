namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CTest
    {
        public int TestId { get; set; }

        public int UserId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public double? Score { get; set; }

        public string Status { get; set; } = null!;

    }
}
