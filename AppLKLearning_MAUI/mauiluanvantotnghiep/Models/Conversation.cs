using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class Conversation
    {
        public int ConversationID { get; set; }
        public int AdminID { get; set; }
        public string AdminName { get; set; } // Tên admin
        public int UserID { get; set; }
        public string UserName { get; set; } // Tên user
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
