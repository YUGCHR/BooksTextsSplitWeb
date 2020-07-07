using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class AllBooksIds
    {
        public AllBooksIds(List<TextSentence> firstBookSentenceIds, int sortedBooksIdsIndex)
        {
            AllFirstBookSentenceIds = firstBookSentenceIds;
            SortedBooksIdsLength = sortedBooksIdsIndex;
        }
        public List<TextSentence> AllFirstBookSentenceIds { get; set; }
        public int SortedBooksIdsLength { get; set; }
    }
}
