namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CUserPackage
    {
        public int PackageId { get; set; }

        public int UserId { get; set; }
        public string? PackageName { get; set; } 

        public DateTime RegistrationDate { get; set; }

        public DateTime? ExpirationDate { get; set; }



        // Property hiển thị, nếu ExpirationDate là null thì hiển thị "vĩnh viễn"
        public string ExpirationDateDisplay
        {
            get
            {
                return ExpirationDate.HasValue
                    ? ExpirationDate.Value.ToString("dd/MM/yyyy")
                    : "vĩnh viễn";
            }
        }
    }
}
