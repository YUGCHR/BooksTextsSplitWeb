using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BooksNamesExistInDb
    {
        public List<BooksNamesSortByLanguageIdSortByBookId> BooksNamesIds { get; set; }
        //public List<TextSentence> AllBookNamesSortedByIds { get; set; }        
    }

    public class BooksNamesSortByLanguageIdSortByBookId
    {
        public int BookId { get; set; }
        public IList<BooksNamesSortByLanguageId> BooksDescriptions { get; set; }
    }

    public class BooksNamesSortByLanguageId
    {
        public int LanguageId { get; set; }
        public TextSentence.BookPropertiesInLanguage BooksDescriptionsDetails { get; set; }
        public TextSentence Sentence { get; set; } // will not be used, for compatibility only
    }
}
