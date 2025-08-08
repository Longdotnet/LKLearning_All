using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public string VerificationCode { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string Signature { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public int? CertificateTypeId { get; set; }

    public virtual CertificateType? CertificateType { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
