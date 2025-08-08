using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class VocabularyMeaning
    {
        public int VocabularyMeaningId { get; set; }
        public int VocabularyId { get; set; }
        public string Meaning { get; set; }
        public string ExampleSentence { get; set; }
        public string TranslatedMeaning { get; set; }
        public string TranslatedExampleSentence { get; set; }
        public string Synonyms { get; set; }
        public string Antonyms { get; set; }
        public string PartOfSpeech { get; set; }
    }
}
