namespace WEBAPI.Services.Bannerbear
{
    public class BannerRequest
    {

        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string StudentName { get; set; }
        public string Subtitle { get; set; }
        public string Signature { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? ExpirationDate { get; set; }
        public string VerificationCode { get; set; }


    }
}
