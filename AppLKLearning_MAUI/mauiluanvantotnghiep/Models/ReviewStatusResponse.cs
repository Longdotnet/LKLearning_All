using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class ReviewStatusResponse
    {
        public string Status { get; set; }    // e.g. "NotReviewed", "Reviewed", "IncompleteLessons"
        public string Message { get; set; }   // e.g. "Bạn chưa viết nhận xét cho khóa học này"
    }
}
