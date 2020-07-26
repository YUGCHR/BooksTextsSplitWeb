using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class BooksVersionsExistInDb
    {
        public List<BooksVersionsGroupedByBookId> AllVersionsOfBooksNames { get; set; }
        //public List<TextSentence> AllBookNamesSortedByIds { get; set; }        
    }

    public class BooksVersionsGroupedByBookId
    {
        public int BookId { get; set; }
        public IList<TextSentence> Sentences { get; set; }
    }
}

