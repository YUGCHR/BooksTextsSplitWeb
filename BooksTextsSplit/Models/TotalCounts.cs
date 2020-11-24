﻿using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Newtonsoft.Json;
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
        [JsonProperty(PropertyName = "booksIdsCount")]
        public int BooksIdsCount { get; private set; }

        [JsonProperty(PropertyName = "versionsCountLanguageId")]
        public int VersionsCountLanguageId { get; private set; }

        [JsonProperty(PropertyName = "paragraphsCountLanguageId")]
        public int ParagraphsCountLanguageId { get; private set; }

        [JsonProperty(PropertyName = "sentencesCountLanguageId")]
        public int SentencesCountLanguageId { get; private set; }

        [JsonProperty(PropertyName = "allBooksIdsList")]
        public int[] AllBooksIdsList { get; private set; }

        [JsonProperty(PropertyName = "versionsCountsInBooskIds")]
        public int[] VersionsCountsInBooskIds { get; private set; }

        [JsonProperty(PropertyName = "paragraphsCountsInBooskIds")]
        public int[] ParagraphsCountsInBooskIds { get; private set; }

        [JsonProperty(PropertyName = "sentencesCountsInBooskIds")]
        public int[] SentencesCountsInBooskIds { get; private set; }

        [JsonProperty(PropertyName = "totalRecordsCount")]
        public int TotalRecordsCount { get; set; }
    }
}
