using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
  public  class Orders
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int PaymentMethodId { get; set; }
        public string Name { get; set; }


        // Thêm các thuộc tính để ánh xạ biểu tượng
        public string StatusIcon { get; set; }
        public string GatewayIcon { get; set; }

        // prop mới: tên gói cước lấy từ enpoint 
        public string PackageName { get; set; }

    }
}
