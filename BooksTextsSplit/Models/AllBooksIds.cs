using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class AllBooksIds
    {
        public AllBooksIds(List<TextSentence> bookNamesSortedByIds, int sortedBooksIdsIndex)
        {
            AllBookNamesSortedByIds = bookNamesSortedByIds;
            //AllEngBooksNames = engBooksNames;
            //AllRusBooksNames = rusBooksNames;
            SortedBooksIdsLength = sortedBooksIdsIndex;
        }
        public List<TextSentence> AllBookNamesSortedByIds { get; set; }
        //public List<TextSentence> AllEngBooksNames { get; set; }
        //public List<TextSentence> AllRusBooksNames { get; set; }
        public int SortedBooksIdsLength { get; set; }
    }
}
