using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class PackageInclusion
{
    public int ParentPackageId { get; set; }

    public int IncludedPackageId { get; set; }

    public DateTime? DateAdd { get; set; }

    public virtual Package IncludedPackage { get; set; } = null!;

    public virtual Package ParentPackage { get; set; } = null!;
}
