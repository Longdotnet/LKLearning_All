using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
   public  class UserPackageRegistration
    {
        public int PackageId { get; set; }
        public int UserId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string ExpirationDateDisplay { get; set; }

    }
}
