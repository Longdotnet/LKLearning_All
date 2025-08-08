using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class LoginResponse
    {

        public string Token { get; set; }

        public string Role { get; set; }
    }
}
