using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class UserProfile
    {
        public string? Fullname { get; set; }
        public string Email { get; set; } 
        public int? Age { get; set; }
        public string? Phone { get; set; }
        public bool? Gender { get; set; }        // nullable bool
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? AvatarUrl { get; set; } // URL của ảnh đại diện
        public int? AvatarId { get; set; } // ID của ảnh đại diện
    }
}
