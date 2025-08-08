using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels.AppConfig
{
     
    public static class AppConfig
    {
        //nơi định nghĩa url gốc.Dễ đổi sau này nếu có đổi server
        public static string BaseUrl { get; set; } = "https://testwebapi.somee.com";
     
        //https://webluanvan.somee.com
    }
}
