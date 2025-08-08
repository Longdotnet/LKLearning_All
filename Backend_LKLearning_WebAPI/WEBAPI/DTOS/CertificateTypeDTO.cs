using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class CertificateTypeDTO
{
    public int CertificateTypeId { get; set; }

    public string TypeName { get; set; } = null!;

}
