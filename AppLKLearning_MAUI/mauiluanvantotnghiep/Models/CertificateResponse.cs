using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class CertificateResponse
    {
        public int CertificateId { get; set; }
        public int UserId { get; set; }
        public string? Fullname { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? VerificationCode { get; set; }
        public string ImageUrl { get; set; }
        public string Signature { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? CertificateTypeId { get; set; }
        public string? CertificateTypeName { get; set; }
    }
}
