namespace WEBAPI.DTOS
{
    public class ThongTinUserDTO
    {
        public int UserId { get; set; }
        public string? Fullname { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public int? Age { get; set; }

        public string? Phone { get; set; }

        public bool? Gender { get; set; }

        public DateOnly? DateofBirth { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public string? AvatarUrl { get; set; }

    }
}
