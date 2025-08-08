using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class CertificateType
{
    public int CertificateTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
