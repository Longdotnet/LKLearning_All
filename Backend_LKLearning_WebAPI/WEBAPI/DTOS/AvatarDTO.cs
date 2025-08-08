namespace WEBAPI.DTOS
{
    public class AvatarDTO
    {
        public int AvatarId { get; set; }

        public string UrlPath { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }
    }
}
