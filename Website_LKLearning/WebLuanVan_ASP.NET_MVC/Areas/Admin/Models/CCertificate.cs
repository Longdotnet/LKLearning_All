using System;
using System.Collections.Generic;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{

    public class CCertificate
    {
        public int CertificateId { get; set; }

        public int UserId { get; set; }
        public string? Fullname { get; set; }

        public int CourseId { get; set; }
        public string? CourseName { get; set; } = null!;

        public string VerificationCode { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public string Signature { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public int? CertificateTypeId { get; set; }
        public string? CertificateTypeName { get; set; }


    }
}
