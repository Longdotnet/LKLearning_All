using Humanizer.Inflections;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CVocabularyCategoryMapping
    {
        public int VocabularyId { get; set; }

        public int VocabularyCategoryId { get; set; }

        public DateTime? DateAdded { get; set; }
      
        public CVocabularyCategoryMapping()
        {
            DateAdded = DateTime.Now;
        }
        public CVocabularyCategoryMapping(int vocabularyId, int vocabularyCategoryId)
        {
            VocabularyId = vocabularyId;
            VocabularyCategoryId = vocabularyCategoryId;
            DateAdded = DateTime.Now;
        }

    }
}
