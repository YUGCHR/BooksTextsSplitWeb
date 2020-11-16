﻿using BooksTextsSplit.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace BooksTextsSplit.Services
{
    public interface IControllerDataManager
    {
        public Task<bool> RemoveKeyLanguageId(int languageId);
        public Task<TotalCounts> FetchTotalCountsFromCache(int languageId);
        public Task<BooksVersionsExistInDb> FetchBookNameVersions(string where, int whereValue, int bookId);
        public Task<BooksNamesExistInDb> FetchBooksNamesIds(string where, int whereValue, int startUploadVersion);
        public Task<BooksPairTextsFromDb> FetchBooksPairTexts(string where1, int where1Value, string where2, int where2Value);


    }

    static class Constants
    {
        public static int FieldsCountForTotalCounts = 4;

        public static string FieldNameLanguageId = "languageId";
        public static string FieldNameBookSentenceId = "bookSentenceId";
        public static string FieldNameBookIdProperty = "BookId"; // but no
        public static string FieldNameBooksId = "bookId";
        public static string FieldNameParagraphId = "paragraphId";
        public static string FieldNameUploadVersion = "uploadVersion";

        public static string GetTotalCountBase = "GetTotalCountWhereLanguageId:";
        public static string GetBooksIdsArray = "GetBooksIdsArrayAndLanguageId:";
        public static string GetTotalCountsArrayBase2 = "GetTotalCountsArray2AndLanguageId:";
        public static string GetTotalCountsArrayBase3 = "GetTotalCountsArray3AndLanguageId:";
        public static string GetTotalCountsArrayBase4 = "GetTotalCountsArray4AndLanguageId:";
    }

    public class ControllerDataManager : IControllerDataManager
    {
        private readonly ILogger<ControllerDataManager> _logger;
        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;

        public ControllerDataManager(
            ILogger<ControllerDataManager> logger,
            ICosmosDbService cosmosDbService,
            IAccessCacheData access)
        {
            _logger = logger;
            _access = access;
            _context = cosmosDbService;

        }

        public async Task<bool> RemoveKeyLanguageId(int languageId)
        {
            string key = Constants.GetTotalCountBase + languageId;
            //may be add keys from versions when they are deleted
            return await _access.RemoveAsync(key);
        }

        public async Task<TotalCounts> FetchTotalCountsFromCache(int languageId)
        {
            // добавить в totalCounts названия полей и загружать их с сервера
            string keyTotalCount = Constants.GetTotalCountBase + languageId.ToString(); // "GetTotalCountWhereLanguageId:"            
            string keyBooksIds = Constants.GetBooksIdsArray + languageId.ToString(); // "GetBooksIdsArrayAndLanguageId:"
            string keyArrays2 = Constants.GetTotalCountsArrayBase2 + languageId.ToString();
            string keyArrays3 = Constants.GetTotalCountsArrayBase3 + languageId.ToString();
            string keyArrays4 = Constants.GetTotalCountsArrayBase4 + languageId.ToString();

            //for Debug Db only - start
            bool removeKeyResult = await _access.RemoveAsync(keyTotalCount);
            bool removeKeyResult1 = await _access.RemoveAsync(keyBooksIds);
            bool removeKeyResult2 = await _access.RemoveAsync(keyArrays2);
            bool removeKeyResult3 = await _access.RemoveAsync(keyArrays3);
            bool removeKeyResult4 = await _access.RemoveAsync(keyArrays4);
            //for Debug Db only - end 

            // исправить запрос абзацев - складывать по главам или доставать лист и складывать пока не равно предыдущему
            int countsExist = await _access.FetchObjectAsync<int>(keyTotalCount, () => FetchSentencesCountsFromDb(languageId));

            //SELECT DISTINCT VALUE c.bookId FROM c WHERE c.bookSentenceId = 1 AND c.languageId = 1
            string queryString = $"SELECT DISTINCT c.{Constants.FieldNameBooksId} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameBookSentenceId} = {1}";
            int[] allBooksIds = await _access.FetchObjectAsync<int[]>(keyBooksIds, () => FetchItemsArrayFromDb(queryString, "BookId"));
            int allBooksIdsLength = allBooksIds.Length;
            string stringBooksIds = String.Join(",", allBooksIds.Select(p => p.ToString())); // ToString().ToArray()

            //SELECT VALUE c.uploadVersion FROM c where c.bookSentenceId = 1 AND c.languageId = 1 AND c.bookId IN (77, 88, 39, 37)                                                                                             
            //SELECT DISTINCT VALUE c.uploadVersion FROM c where c.bookSentenceId = 1 AND c.languageId = 1 AND c.bookId = 77
            queryString = $"SELECT DISTINCT c.{Constants.FieldNameUploadVersion} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameBookSentenceId} = {1} AND c.bookId = ";
            int[] versionsCounts = await _access.FetchObjectAsync<int[]>(keyArrays2, () => FetchItemsArrayFromDb(queryString, "UploadVersion", allBooksIds));






            //SELECT DISTINCT c.chapterId FROM c where c.languageId = 0 AND c.uploadVersion = 1 AND c.bookId = 77
            //SELECT DISTINCT VALUE(c.paragraphId) FROM c where c.languageId = 0 AND c.uploadVersion = 1 AND c.bookId = 77 AND c.chapterId = 1

            //SELECT c.paragraphId FROM c where c.languageId = 0 AND c.uploadVersion = 1 AND c.bookId = 77
            queryString = $"SELECT c.{Constants.FieldNameParagraphId} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameUploadVersion} = 1 AND c.bookId = ";
            int[] paragraphsCounts = await _access.FetchObjectAsync<int[]>(keyArrays3, () => FetchItemsArrayFromDb(queryString, "paragraphId",  allBooksIds));
            //VALUE COUNT()





            //SELECT VALUE COUNT(c.bookSentenceId) FROM c where c.languageId = 0 AND c.bookId = 77
            queryString = $"SELECT VALUE COUNT(c.{Constants.FieldNameBookSentenceId}) FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.bookId = ";
            int[] sentencesCounts = await _access.FetchObjectAsync<int[]>(keyArrays4, () => FetchItemsArrayFromDb(queryString, allBooksIds));

            TotalCounts totalCountsFromCache = new TotalCounts(allBooksIds, versionsCounts, paragraphsCounts, sentencesCounts);
            return totalCountsFromCache;
        }

        public async Task<int> FetchSentencesCountsFromDb(int languageId) // always fetch data from db as version of book of language
        {
            //SELECT VALUE COUNT(1) FROM c where c.languageId=0
            int languageSentencesCount = await _context.GetCountItemAsync($"SELECT VALUE COUNT(1) FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId}") ?? 0;
            return languageSentencesCount;
        }
        
        public async Task<int[]> FetchItemsArrayFromDb(string queryString, string propName) // always fetch data from db as version of book of language
        {
            List<TextSentence> allBooksIds = await _context.GetItemsListAsync<TextSentence>(queryString);
            var result = allBooksIds.Select(a => (int)a.GetType().GetProperty(propName).GetValue(a, null)).ToArray();
            return result;
        }

        public async Task<int[]> FetchItemsArrayFromDb(string queryString, string propName, int[] allBooksIds) // always fetch data from db as version of book of language
        {
            int[] allUploadedVersionsCounts = new int[allBooksIds.Length];
            for (int i = 0; i < allBooksIds.Length; i++)
            {
                List<TextSentence> allUploadedVersions = await _context.GetItemsListAsync<TextSentence>(queryString + allBooksIds[i].ToString());
                allUploadedVersionsCounts[i] = (allUploadedVersions.Select(a => (int)a.GetType().GetProperty(propName).GetValue(a, null)).ToArray()).Count();
            }            
            return allUploadedVersionsCounts;
        }

        public async Task<int[]> FetchItemsArrayFromDb(string queryString, int[] allBooksIds)
        {
            int[] allUploadedVersions = new int[allBooksIds.Length];
            for (int i = 0; i < allBooksIds.Length; i++)
            {
                allUploadedVersions[i] = await _context.GetItemCountAsync<int>(queryString + allBooksIds[i].ToString());
            }
            return allUploadedVersions;
        }





        public async Task<BooksVersionsExistInDb> FetchBookNameVersions(string where, int whereValue, int bookId)
        {
            string bookSentenceIdKey = where + ":" + whereValue.ToString();
            List<TextSentence> requestedSelectResult = await _access.FetchObjectAsync<List<TextSentence>>(bookSentenceIdKey, () => FetchBooksNamesFromDb(where, whereValue));

            IEnumerable<IGrouping<int, TextSentence>> languageIdGrouping = requestedSelectResult.Where(r => r.BookId == bookId).ToList().GroupBy(r => r.LanguageId);

            BooksVersionsExistInDb foundBooksVersion = new BooksVersionsExistInDb
            {
                SelectedBookIdAllVersions = languageIdGrouping.Select(p => new SelectedBookIdGroupByLanguageId
                {
                    LanguageId = p.Key,
                    Sentences = p.OrderBy(v => v.UploadVersion).Select(s => s).ToList()
                }
                ).OrderBy(s => s.LanguageId).ToList()
            };
            #region for_memory_grouping_in_grouping
            // на память - группировка по LanguageId внутри группировки по BookId
            IEnumerable<IGrouping<int, TextSentence>> allVersionsPairings = requestedSelectResult.GroupBy(r => r.BookId);
            BooksVersionsExistInDb_Memory foundBooksVersion_Memory = new BooksVersionsExistInDb_Memory
            {
                AllVersionsOfBooksNames = allVersionsPairings.Select(p => new BooksVersionsGroupedByBookIdGroupByLanguageId
                {
                    BookId = p.Key,
                    BookVersionsDescriptions = p.GroupBy(l => l.LanguageId).Select(g => new BooksVersionsGroupByLanguageId_Memory
                    {
                        LanguageId = g.Key,
                        Sentences = g.OrderBy(v => v.UploadVersion).Select(t => t).ToList()
                    }
                    ).OrderBy(s => s.LanguageId).ToList()
                }
                ).ToList()
            };
            #endregion
            return foundBooksVersion;
        }





        public async Task<BooksNamesExistInDb> FetchBooksNamesIds(string where, int whereValue, int startUploadVersion)
        {
            string bookSentenceIdKey = where + ":" + whereValue.ToString();
            List<TextSentence> requestedSelectResult = await _access.FetchObjectAsync<List<TextSentence>>(bookSentenceIdKey, () => FetchBooksNamesFromDb(where, whereValue));

            List<TextSentence> toSelectBookNameFromAll = requestedSelectResult.Where(r => r.UploadVersion == startUploadVersion).ToList();

            IEnumerable<IGrouping<int, TextSentence>> allBooksNamesPairings = toSelectBookNameFromAll.GroupBy(r => r.BookId);
            BooksNamesExistInDb foundBooksIds = new BooksNamesExistInDb
            {
                BookNamesVersion1SortedByIds = allBooksNamesPairings.Select(p => new BooksNamesSortByLanguageIdSortByBookId
                {
                    BookId = p.Key,
                    BooksDescriptions = p.OrderBy(s => s.LanguageId).Select(s => new BooksNamesSortByLanguageId { LanguageId = s.LanguageId, Sentence = s }).ToList()
                }
                ).ToList()
            };

            return foundBooksIds;
        }






        public async Task<List<TextSentence>> FetchBooksNamesFromDb(string where, int whereValue)
        {
            // bool areWhereOrderByRealProperties = true; //AreParamsRealTextSentenceProperties(where, orderBy); - it is needs to add checking of parameters existing 

            List<TextSentence> requestedSelectResult = (await _context.GetItemsAsync
                ($"SELECT * FROM c WHERE c.{where} = {whereValue}"))
                .OrderBy(li => li.BookId) // if it remove the sort, the both methods will be the same
                .ThenBy(uv => uv.UploadVersion)
                .ThenBy(bi => bi.LanguageId)
                .ToList();

            // Set List to Redis
            string createdKeyNameFromRequest = where + ":" + whereValue.ToString(); //выдачу из базы сохранить как есть, с ключом bookSentenceId:1                                                                                
            await _access.SetObjectAsync(createdKeyNameFromRequest, requestedSelectResult, TimeSpan.FromDays(1));

            return requestedSelectResult;
        }




        public async Task<BooksPairTextsFromDb> FetchBooksPairTexts(string where1, int where1Value, string where2, int where2Value)
        {
            string booksPairTextsKey = where1 + ":" + where1Value.ToString();
            List<TextSentence> requestedSelectResult = await _access.FetchObjectAsync<List<TextSentence>>(booksPairTextsKey, () => FetchBooksTextsFromDb(where1, where1Value));

            //where2 must be uploadVersion for the next grouping
            //TODO get UploadVersion from where2
            IEnumerable<IGrouping<int, TextSentence>> languageIdGrouping = requestedSelectResult.Where(r => r.UploadVersion == where2Value).ToList().GroupBy(r => r.LanguageId);

            BooksPairTextsFromDb foundBooksPairTexts = new BooksPairTextsFromDb // selectedBooksPairTexts
            {
                SelectedBooksPairTexts = languageIdGrouping.Select(p => new BooksPairTextsGroupByLanguageId
                {
                    LanguageId = p.Key,
                    Sentences = p.OrderBy(v => v.BookSentenceId).Select(s => s).ToList()
                }
                ).OrderBy(s => s.LanguageId).ToList()
            };

            return foundBooksPairTexts;
        }


        public async Task<List<TextSentence>> FetchBooksTextsFromDb(string where, int whereValue)
        {
            // bool areWhereOrderByRealProperties = true; //AreParamsRealTextSentenceProperties(where, orderBy); - it is needs to add checking of parameters existing 

            List<TextSentence> requestedSelectResult = (await _context.GetItemsAsync
                ($"SELECT * FROM c WHERE c.{where} = {whereValue}"))
                .OrderBy(uv => uv.UploadVersion)
                .ThenBy(bi => bi.LanguageId)
                .ThenBy(si => si.BookSentenceId)
                .ToList();

            // Set List to Redis
            string createdKeyNameFromRequest = where + ":" + whereValue.ToString(); //выдачу из базы сохранить как есть, с ключом bookId:(selected BookId)                                                                                
            await _access.SetObjectAsync(createdKeyNameFromRequest, requestedSelectResult, TimeSpan.FromDays(1));

            return requestedSelectResult;
        }




    }
}

