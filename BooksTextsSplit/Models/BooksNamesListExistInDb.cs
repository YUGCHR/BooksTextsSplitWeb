using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BooksNamesListExistInDb
    {
        public List<SentencesSortByLanguageIdSortByBookId> Version1BookNamesSortedByIds { get; set; }
        //public List<TextSentence> AllBookNamesSortedByIds { get; set; }        
    }

    public class SentencesSortByLanguageIdSortByBookId
    {
        public int BookId { get; set; }
        public IList<SentenceSortByLanguageId> BooksDescriptions { get; set; }
    }

    public class SentenceSortByLanguageId
    {
        public int LanguageId { get; set; }
        public TextSentence Sentence { get; set; }
    }
}
