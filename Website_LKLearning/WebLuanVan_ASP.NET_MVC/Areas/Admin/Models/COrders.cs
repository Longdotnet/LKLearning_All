namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class COrders
    {
        public string OrderId { get; set; } = null!;

        public string? TransactionId { get; set; }

        public string Status { get; set; } = null!;

        public int UserId { get; set; }
        public string FullName { get; set; }

        public int PackageId { get; set; }
        public string PackageName { get; set; }



        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; } // Add this property



        public int PaymentMethodId { get; set; }
        public string Name { get; set; }

    }
}
