using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class VocabularyCategory
    {
        public int VocabularyCategoryId { get; set; }
        public string VocabularyCategoryName { get; set; }
        public string VocabularyCategoryDescription { get; set; }
        public string UrlImage { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? VocabularyCount { get; set; }


        // Thuộc tính màu nền, đánh dấu JsonIgnore để không ảnh hưởng khi deserialize
        [JsonIgnore]
        public Color BackgroundColor { get; set; }
    }
}
