using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class VerifyCertificateRequest
    {
        public string VerifyCode { get; set; }
    }

    public class VerifyCertificateResponse
    {
        public string Message { get; set; }
        public string Expiration { get; set; }
        public string ImageUrl { get; set; }
    }
}
