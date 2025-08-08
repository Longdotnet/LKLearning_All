using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class VocabularyWithMeanings
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; }
        public string Pronunciation { get; set; }
        public string AudioUrlUk { get; set; }
        public string AudioUrlUs { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VocabularyMeaning>? Meanings { get; set; }
    }
}
