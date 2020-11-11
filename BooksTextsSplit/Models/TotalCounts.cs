using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class TotalCounts
    {
        public TotalCounts(int[] allBooksIds, int[] versionsCounts, int[] paragraphsCounts, int[] sentencesCounts)
        {
            AllBooksIdsList = allBooksIds;
            VersionsCountsInBooskIds = versionsCounts;
            ParagraphsCountsInBooskIds = paragraphsCounts;
            SentencesCountsInBooskIds = sentencesCounts;

            BooksIdsCount = AllBooksIdsList.Count();
            VersionsCountLanguageId = Enumerable.Sum(VersionsCountsInBooskIds);
            ParagraphsCountLanguageId = Enumerable.Sum(ParagraphsCountsInBooskIds);
            SentencesCountLanguageId = Enumerable.Sum(SentencesCountsInBooskIds);
        }
        public int BooksIdsCount { get; private set; }
        public int VersionsCountLanguageId { get; private set; }
        public int ParagraphsCountLanguageId { get; private set; }
        public int SentencesCountLanguageId { get; private set; }
        public int[] AllBooksIdsList { get; private set; }
        public int[] VersionsCountsInBooskIds { get; private set; }
        public int[] ParagraphsCountsInBooskIds { get; private set; }
        public int[] SentencesCountsInBooskIds { get; private set; }                
    }
}
