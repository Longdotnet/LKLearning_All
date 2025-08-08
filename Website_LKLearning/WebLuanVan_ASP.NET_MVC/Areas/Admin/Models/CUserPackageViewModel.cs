namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CUserPackageViewModel
    {
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public string Fullname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string PackageName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string ExpirationDateDisplay { get; set; }   
    }

    public class UserPackageKey
    {
        public int UserId { get; set; }
        public int PackageId { get; set; }
    }
}