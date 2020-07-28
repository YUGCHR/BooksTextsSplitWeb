using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BooksVersionsExistInDb
    {
        public List<BooksVersionsGroupedByBookIdGroupByLanguageId> AllVersionsOfBooksNames { get; set; }
        //public List<TextSentence> AllBookNamesSortedByIds { get; set; }        
    }

    public class BooksVersionsGroupedByBookIdGroupByLanguageId
    {
        public int BookId { get; set; }
        public List<BooksVersionsGroupByLanguageId> BookVersionsDescriptions { get; set; } 
    }

    public class BooksVersionsGroupByLanguageId
    {
        public int LanguageId { get; set; }
        public IList<TextSentence> Sentences { get; set; }
    }
}

