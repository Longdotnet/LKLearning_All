using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class TransactionDetail
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; }
        public int Idgateway { get; set; }
        public string GatewayDescription { get; set; }
    }
}
