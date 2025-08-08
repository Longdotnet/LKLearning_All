using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class Certificate
    {
        public string ImageUrl { get; set; }
        public int CertificateId { get; set; }
    }

    public class CreateCertificateRequest
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string StudentName { get; set; }
        public string Subtitle { get; set; }
        public string Signature { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string VerificationCode { get; set; }
    }
}
