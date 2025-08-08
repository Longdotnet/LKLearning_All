using System.Text.Json.Serialization;

namespace WEBAPI.Services.Solana
{
    public class MemoData
    {

        [JsonPropertyName("userid")]
        public int UserId { get; set; }

        [JsonPropertyName("courseid")]
        public int CourseId { get; set; }

        [JsonPropertyName("imageurl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("expirationDate")]
        public DateTime? ExpirationDate { get; set; }
    }
}
