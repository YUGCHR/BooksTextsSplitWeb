using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BooksVersionsExistInDb
    {
        public List<BooksVersionsGroupedByLanguageIdGroupedByBookId> AllVersionsOfBooksNames { get; set; }
        //public List<TextSentence> AllBookNamesSortedByIds { get; set; }        
    }

    public class BooksVersionsGroupedByLanguageIdGroupedByBookId
    {
        public int BookId { get; set; }
        public IList<BooksVersionsGroupedByLanguageId> BooksDescriptionsVersions { get; set; }
    }

    public class BooksVersionsGroupedByLanguageId
    {
        public int LanguageId { get; set; }
        public IList<TextSentence> Sentences { get; set; }
    }
}

