using System.Text.Json.Serialization;

namespace WebLuanVan_ASP.NET_MVC.Areas.Login.Models.DTOS
{
    public class TokenResultDTO
    {
        public string Token { get; set; }
        [JsonPropertyName("role")]
        public string RoleId { get; set; }
        public UserDTO User { get; set; }

    }
}
