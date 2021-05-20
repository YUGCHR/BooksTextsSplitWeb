using System.Collections.Generic;

namespace BooksTextsSplit.Library.Models
{
    public class BooksVersionsExistInDb_Memory
    {
        public List<BooksVersionsGroupedByBookIdGroupByLanguageId> AllVersionsOfBooksNames { get; set; }
        //public List<TextSentence> AllBookNamesSortedByIds { get; set; }        
    }

    public class BooksVersionsGroupedByBookIdGroupByLanguageId
    {
        public int BookId { get; set; }
        public List<BooksVersionsGroupByLanguageId_Memory> BookVersionsDescriptions { get; set; } 
    }

    public class BooksVersionsGroupByLanguageId_Memory
    {
        public int LanguageId { get; set; }
        public IList<TextSentence> Sentences { get; set; }
    }
}

