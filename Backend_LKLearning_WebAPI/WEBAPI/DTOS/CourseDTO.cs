namespace WEBAPI.DTOS
{
    public class CourseDTO
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = null!;

        public string? Description { get; set; }

        public int DurationInMonths { get; set; }

        public int LevelId { get; set; }
        public string? UrlImage { get; set; }

        public int CategoryId { get; set; }

        public int? PackageId { get; set; }

        public int? CertificateDurationDays { get; set; }

    }
}
