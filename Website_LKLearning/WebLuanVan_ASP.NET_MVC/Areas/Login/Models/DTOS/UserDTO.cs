using System.Text.Json.Serialization;

namespace WebLuanVan_ASP.NET_MVC.Areas.Login.Models.DTOS
{
    public class UserDTO
    {
        public int UserId { get; set; }

        public string? Fullname { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public int? Age { get; set; }

        public string? Phone { get; set; }

        public bool? Gender { get; set; }
    
        public DateOnly? DateofBirth { get; set; }

        public DateOnly? LastLoginDate { get; set; }

       

    }
}
