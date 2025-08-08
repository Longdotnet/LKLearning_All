namespace WEBAPI.DTOS
{
    public class PackageDTO
    {
        public int PackageId { get; set; }

        public string PackageName { get; set; } = null!;

        public int? DurationDay { get; set; }
        public string? UrlImage { get; set; }

        public decimal? Price { get; set; }
        public List<int> IncludedPackageIds { get; set; } = new List<int>();
    }
}
