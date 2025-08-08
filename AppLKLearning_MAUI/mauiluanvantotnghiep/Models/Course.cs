using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public int DurationInMonths { get; set; }
        public int LevelId { get; set; }
        public string UrlImage { get; set; }
        public int CategoryId { get; set; }
        public int PackageId { get; set; }




    }
}
