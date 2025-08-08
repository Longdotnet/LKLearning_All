using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class Banner
    {
        public int BannerId { get; set; }
        public string BannerTitle { get; set; }
        public string BannerSubtitle { get; set; }
        public string BannerDescription { get; set; }
        public string BannerImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
