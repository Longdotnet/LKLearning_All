namespace WebLuanVan_ASP.NET_MVC.Areas.Login.Models.DTOS
{
    public class CapNhatThonTinUserDTO
    {
        public string Fullname { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Phone { get; set; } = string.Empty;
        public bool Gender { get; set; } 
        public DateOnly DateOfBirth { get; set; }
    }
}
