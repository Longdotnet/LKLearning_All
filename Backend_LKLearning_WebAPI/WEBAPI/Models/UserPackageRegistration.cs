using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class UserPackageRegistration
{
    public int PackageId { get; set; }

    public int UserId { get; set; }

    public DateTime RegistrationDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public virtual Package Package { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
