using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Package
{
    public int PackageId { get; set; }

    public string PackageName { get; set; } = null!;

    public int? DurationDay { get; set; }

    public decimal? Price { get; set; }

    public string? UrlImage { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PackageInclusion> PackageInclusionIncludedPackages { get; set; } = new List<PackageInclusion>();

    public virtual ICollection<PackageInclusion> PackageInclusionParentPackages { get; set; } = new List<PackageInclusion>();

    public virtual ICollection<UserPackageRegistration> UserPackageRegistrations { get; set; } = new List<UserPackageRegistration>();
}
