using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    // Model cho kết quả verify ZaloPay
    public class ZaloPayVerifyResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
