using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class AllBooksIds
    {
        public AllBooksIds(int[] sortedBookIds, int sortedBookIdsLength)
        {
            AllSortedBooksIds = sortedBookIds;
            SortedBooksIdsLength = sortedBookIdsLength;
        }
        public int[] AllSortedBooksIds { get; set; }
        public int SortedBooksIdsLength { get; set; }
    }
}
