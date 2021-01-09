using BackgroundTasksQueue.Helpers;
using BooksTextsSplit.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BackgroundTasksQueue.Services
{
    #region Declarations
    public interface IControllerDataManager
    {
        public Task<TaskUploadPercents> IsExistBackgroundUpdateKeys();
        public Task<bool> RemoveTotalCountWhereLanguageId(int languageId);
        public Task<int> TotalRecordsCountWhereLanguageId(int languageId);
        public Task<TotalCounts> FetchTotalCounts(int languageId, [CallerMemberName] string currentMethodNameName = "");
        public TaskUploadPercents CreateTaskGuidPercentsKeys(string guid, TextSentence bookDescription = null, int textSentencesLength = 0, [CallerMemberName] string currentMethodNameName = "");
        public Task<TaskUploadPercents> GetTaskState(TaskUploadPercents taskStateCurrent);
        public Task<bool> SetTaskState(TaskUploadPercents uploadPercents, int keysExistingTimeFactor = 1);
        public Task<TaskUploadPercents> FetchUploadTaskPercents(string taskGuid);
        public Task<BooksVersionsExistInDb> FetchBookNameVersions(string where, int whereValue, int bookId);
        public Task<BookIdsListExistInDv> FetchBooksNamesVersionsProperties();
        public Task<BooksPairTextsFromDb> FetchBooksPairTexts(string where1, int where1Value, string where2, int where2Value);
    }

    static class Constants
    {
        public static int FieldsCountForTotalCounts = 4;
        public static int RecordActualityLevel = 6; // DELETE!

        public static string FieldNameLanguageId = "languageId";
        public static string FieldNameBookSentenceId = "bookSentenceId";
        public static string FieldNameBookIdProperty = "BookId"; // but no
        public static string FieldNameBooksId = "bookId";
        public static string FieldNameParagraphId = "paragraphId";
        public static string FieldNameUploadVersionProperty = "UploadVersion";
        public static string FieldNameUploadVersion = "uploadVersion";
        public static string FieldNameRecordActualityLevel = "recordActualityLevel";

        public static string GetTotalCountsBase = "GetTotalCountWhereLanguageId:";
        public static string GetBooksIdsArray = "ArrayOfBooksIdsWhereLanguageId:"; // may be remove Get
        public static string GetBookVersionsArray = "ArrayOfBookVersionsWhereLanguageIdAndBookId:"; //"ArrayOfBookVersionsWhereLanguageIdAndBookId:0:73"
        public static string GetParagraphsCountsArray = "GetParagraphsCountsArrayWhereLanguageId:";
        public static string GetSentencesCountsArray = "GetSentencesCountsArrayWhereLanguageId:";
    }

    public class ControllerDataManager : IControllerDataManager
    {
        private readonly ILogger<ControllerDataManager> _logger;
        private readonly IControllerCacheManager _cache;
        private readonly ISettingConstants _constant;


        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;

        public ControllerDataManager(
            ILogger<ControllerDataManager> logger,
            IControllerCacheManager cache,
            ISettingConstants constant,
            ICosmosDbService cosmosDbService,
            IAccessCacheData access)
        {
            _logger = logger;
            _cache = cache;
            _constant = constant;
            _access = access;
            _context = cosmosDbService; // TO REMOVE!
        }

        #endregion

        #region Total Counts

        public async Task<bool> RemoveTotalCountWhereLanguageId(int languageId)
        {
            string key = Constants.GetTotalCountsBase + languageId;
            //may be add keys from versions when they are deleted
            return await _access.RemoveAsync(key);
        }

        public async Task<int> TotalRecordsCountWhereLanguageId(int languageId)
        {
            string keyTotalCounts = Constants.GetTotalCountsBase + languageId.ToString(); // "GetTotalCountWhereLanguageId:"
            //for Debug Db only - start
            bool removeKeyResult = await _access.RemoveAsync(keyTotalCounts);
            //for Debug Db only - end
            // исправить запрос абзацев - складывать по главам или доставать лист и складывать пока не равно предыдущему
            int countsExist = await _access.FetchObjectAsync<int>(keyTotalCounts, () => CountSentencesCountsFromDb(languageId));
            return countsExist;
        }

        public async Task<int> CountSentencesCountsFromDb(int languageId) // always fetch data from db as version of book of language
        {
            //SELECT VALUE COUNT(1) FROM c where c.languageId=0
            int languageSentencesCount = await _context.GetCountAllLanguageItemsAsync(Constants.FieldNameLanguageId, languageId) ?? 0;
            return languageSentencesCount;
        }

        public async Task<TotalCounts> FetchTotalCounts(int languageId, [CallerMemberName] string currentMethodNameName = "")
        {
            // добавить в totalCounts названия полей и загружать их с сервера            
            //string keyBooksIds = Constants.GetBooksIdsArray + languageId.ToString(); // "GetBooksIdsArrayAndLanguageId:"
            string keyBooksIds = Constants.GetBooksIdsArray.KeyBaseAddLanguageId(languageId);
            string keyParagraphsCounts = Constants.GetParagraphsCountsArray + languageId.ToString();
            string keySentencesCounts = Constants.GetSentencesCountsArray + languageId.ToString();
            int level = Constants.RecordActualityLevel; // при старте страницы делать запрос, чтобы получить последний

            await RemoveTotalCountsKeys(languageId, keyBooksIds, keyParagraphsCounts, keySentencesCounts); //for Debug Db only

            int[] allBooksIds = await _cache.FetchAllBooksIds(keyBooksIds, languageId, Constants.FieldNameBookIdProperty, level);
            int allBooksIdsLength = allBooksIds.Length;
            int[] allUploadedVersionsCounts = new int[allBooksIdsLength];

            if (allBooksIds.Length > 0)
            { // versionsCounts
                for (int i = 0; i < allBooksIdsLength; i++)
                {
                    //string keyBookVersionsLangBook = Constants.GetBookVersionsArray + languageId.ToString() + ":" + allBooksIds[i].ToString();
                    string keyBookVersionsLangBook = Constants.GetBookVersionsArray.KeyBaseAddLanguageIdBookId(languageId, allBooksIds[i]);

                    bool removeKeyResult2 = await _access.RemoveAsync(keyBookVersionsLangBook); //for Debug Db only - REMOVE AFTER!

                    int[] uploadedVersions = await _cache.FetchAllBooksIds(keyBookVersionsLangBook, languageId, Constants.FieldNameUploadVersionProperty, level, allBooksIds[i]);

                    for (int j = 0; j < uploadedVersions.Length; j++)
                    {
                        // select totalCounts from chapters and sum them
                    }
                    allUploadedVersionsCounts[i] = uploadedVersions.Length;
                }
            }

            //int[] versionsCounts = await _access.FetchObjectAsync<int[]>(keyVersionsCounts, () => FetchItemsArrayFromDb(languageId, Constants.FieldNameUploadVersionProperty, Constants.RecordActualityLevel, allBooksIds));

            //int[] versionsCounts = new int[] { 5, 5, 5, 5, 5 };
            int[] paragraphsCounts = new int[] { 5, 5, 5, 5, 5 };
            int[] sentencesCounts = new int[] { 5, 5, 5, 5, 5 };

            TotalCounts totalCountsFromCache = new TotalCounts(allBooksIds, allUploadedVersionsCounts, paragraphsCounts, sentencesCounts)
            {
                TotalRecordsCount = await TotalRecordsCountWhereLanguageId(languageId)
            };
            return totalCountsFromCache;
        }

        private async Task<bool> RemoveTotalCountsKeys(int languageId, string keyBooksIds, string keyParagraphsCounts, string keySentencesCounts, [CallerMemberName] string currentMethodNameName = "")
        {
            //for Debug Db only - start            
            bool removeKeyResult1 = await _access.RemoveAsync(keyBooksIds);
            bool removeKeyResult3 = await _access.RemoveAsync(keyParagraphsCounts);
            bool removeKeyResult4 = await _access.RemoveAsync(keySentencesCounts);
            bool removeKeyResult = removeKeyResult1 && removeKeyResult3 && removeKeyResult4;
            //string currentMethodNameName = MethodBase.GetCurrentMethod()?.Name;
            //string currentMethodNameName = ClassesMethodsNames.GetMyMethodName();
            string message = $"Method {currentMethodNameName} with languageId = {languageId} tried to remove keys \n Keys Removing Results = {removeKeyResult1} / {removeKeyResult3} / {removeKeyResult4} \n";
            _logger.LogInformation(message, currentMethodNameName, languageId, removeKeyResult1, removeKeyResult3, removeKeyResult4);
            return removeKeyResult;
            //for Debug Db only - end 
        }

        //string stringBooksIds = String.Join(",", allBooksIds.Select(p => p.ToString())); // ToString().ToArray()

        //SELECT DISTINCT c.chapterId FROM c where c.languageId = 0 AND c.uploadVersion = 1 AND c.bookId = 77
        //SELECT DISTINCT VALUE(c.paragraphId) FROM c where c.languageId = 0 AND c.uploadVersion = 1 AND c.bookId = 77 AND c.chapterId = 1

        //int[] sentencesCounts = await _access.FetchObjectAsync<int[]>(keySentencesCounts, () => FetchItemsArrayFromDb(Constants.FieldNameLanguageId, languageId, allBooksIds));

        //SELECT c.paragraphId FROM c where c.languageId = 0 AND c.uploadVersion = 1 AND c.bookId = 77
        //queryString = $"SELECT c.{Constants.FieldNameParagraphId} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameUploadVersion} = 1 AND c.bookId = ";
        //int[] paragraphsCounts = await _access.FetchObjectAsync<int[]>(keyArrays3, () => FetchItemsArrayFromDb(queryString, "bookContentInChapter.paragraphId",  allBooksIds));
        //VALUE COUNT()


        #endregion

        #region Upload Book

        public async Task<TaskUploadPercents> IsExistBackgroundUpdateKeys()
        {
            string taskGuid = "AAAAB3NzaC1yc2EAAAADAQABAAACAQC4"; // Guid.NewGuid().ToString(); // take from appsetting 
            TaskUploadPercents taskStateCurrent = CreateTaskGuidPercentsKeys(taskGuid);
            return await GetTaskState(taskStateCurrent);
        }

        public TaskUploadPercents CreateTaskGuidPercentsKeys(string guid, TextSentence bookDescription = null, int textSentencesLength = 0, [CallerMemberName] string currentMethodNameName = "")
        {
            if (guid == null)
            {
                string message = $"Attempt of {currentMethodNameName} to start CreateTaskGuidPercentsKeys was failed (guid is null)";
                _logger.LogInformation(message, currentMethodNameName);
                return null;
            }

            string keyBookId = _constant.GetKeyBookId; // bookId
            string keyBookIdAction = _constant.GetKeyBookIdAction; // upload
            string redisKey = keyBookId.KeyBaseRedisKey(keyBookIdAction); // bookId:upload

            string keyTaskPercents = _constant.GetKeyTaskPercents; // percents
            string fieldKeyPercents = guid.KeyBaseRedisKey(keyTaskPercents); // guid:percents

            string keyIsTaskRunning = _constant.GetKeyIsTaskRunning; // isRunning - UNUSED
            string fieldKeyState = guid.KeyBaseRedisKey(keyIsTaskRunning); // guid:isRunning

            bookDescription ??= new TextSentence // if null
            {
                BookId = 0,
                LanguageId = -1,
                UploadVersion = 0
            };

            TaskUploadPercents uploadPercents = new TaskUploadPercents
            {
                IsTaskRunning = false,
                CurrentTaskGuid = guid,
                CurrentUploadingBookId = bookDescription.BookId,
                CurrentUploadingLanguageId = bookDescription.LanguageId,
                CurrentUploadingVersion = bookDescription.UploadVersion,
                DoneInPercents = 0, // do not use
                CurrentUploadingRecord = 0,
                CurrentUploadedRecordRealTime = 0,
                TotalUploadedRealTime = 0,
                RecordsTotalCount = textSentencesLength,
                RedisKey = redisKey,
                FieldKeyPercents = fieldKeyPercents,
                FieldKeyState = fieldKeyState,
                KeysExistingTime = TimeSpan.FromMinutes(_constant.GetPercentsKeysExistingTimeInMinutes)
            };

            return uploadPercents;
        }

        public async Task<TaskUploadPercents> FetchUploadTaskPercents(string taskGuid)
        {
            TaskUploadPercents taskStateCurrent = CreateTaskGuidPercentsKeys(taskGuid);

            bool isUploadInProgress = false;
            while (!isUploadInProgress) // to wait when key taskGuid will appear
            {
                isUploadInProgress = await _access.KeyExistsAsync<TaskUploadPercents>(taskStateCurrent.RedisKey, taskStateCurrent.FieldKeyPercents);
                if (isUploadInProgress)
                {
                    taskStateCurrent = await _cache.FetchTaskState(taskStateCurrent);

                    int previousState = taskStateCurrent.CurrentUploadingRecord;
                    int currentState = previousState;
                    int finishState = taskStateCurrent.RecordsTotalCount - 1;
                    while (currentState == previousState && currentState < finishState)
                    {
                        taskStateCurrent = await _access.GetObjectAsync<TaskUploadPercents>(taskStateCurrent.RedisKey, taskStateCurrent.FieldKeyPercents); // after TimeSpan time the key can disappeared in some reasons
                        if (taskStateCurrent == null)
                        {
                            string message = $"Incomplete UploadTaskPercents pending of RedisKey - {taskGuid} was not completed.";
                            _logger.LogInformation(message, taskGuid);
                            return default;
                        }
                        else
                        {
                            currentState = taskStateCurrent.CurrentUploadingRecord;
                            await Task.Delay(10);
                        }
                    }
                }
                else
                {
                    await Task.Delay(10);
                }
            }
            return taskStateCurrent;
        }

        public async Task<TaskUploadPercents> GetTaskState(TaskUploadPercents taskStateCurrent)
        {
            TaskUploadPercents result = await _cache.FetchTaskState(taskStateCurrent) ?? new TaskUploadPercents
            {
                IsTaskRunning = false
            };
            return result;
        }

        public async Task<bool> SetTaskState(TaskUploadPercents uploadPercents, int keysExistingTimeFactor = 1)
        {
            await _cache.SetTaskGuidKeys(uploadPercents, keysExistingTimeFactor);
            return true;
        }

        #endregion

        #region Select Books Pair

        // Model TextSentence ver.6 
        public async Task<BookIdsListExistInDv> FetchBooksNamesVersionsProperties()
        {
            List<BookPropertiesExistInDb> foundAllBooksIds = await _cache.FetchAllBookIdsLanguageIdsFromCache();

            return new BookIdsListExistInDv
            {
                BooksNamesIds = foundAllBooksIds
            };
        }

        #endregion

        #region LEGACY

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
            // new version
            //IEnumerable<IGrouping<int, TextSentence>> bookIdGroupBy = booksVersionsProperties.GroupBy(r => r.BookId);
            //BooksNamesExistInDb foundBooksIds = new BooksNamesExistInDb
            //{
            //    BooksNamesIds = bookIdGroupBy.Select(p => new BooksNamesSortByLanguageIdSortByBookId
            //    {
            //        BookId = p.Key,
            //        AllBookDescriptions = p.GroupBy(l => l.LanguageId).Select(v => new BooksNamesSortByLanguageId
            //        {
            //            LanguageId = v.Key,
            //            BookVersionsOfBookId = v.Select(t => new BookVersionsTotaICount
            //            {
            //                UploadVersion = t.UploadVersion,
            //                BookDescriptionDetails = t.BookProperties, // оставлен массив названий, исходя из предположения, что версии могут иметь свои аннотации
            //                BookVersionCounts = t.TotalBookCounts
            //            }).ToList()
            //        }).ToList()
            //    }).ToList()
            //};
            #endregion
            return foundBooksVersion;
        }

        #region GroupBy sample
        public async Task<BooksNamesExistInDb> FetchBooksNamesIds_Old(string where, int whereValue, int startUploadVersion)
        {
            string bookSentenceIdKey = where + ":" + whereValue.ToString();
            List<TextSentence> requestedSelectResult = await _access.FetchObjectAsync<List<TextSentence>>(bookSentenceIdKey, () => FetchBooksNamesFromDb(where, whereValue));

            List<TextSentence> toSelectBookNameFromAll = requestedSelectResult.Where(r => r.UploadVersion == startUploadVersion).ToList();

            IEnumerable<IGrouping<int, TextSentence>> allBooksNamesPairings = toSelectBookNameFromAll.GroupBy(r => r.BookId);
            BooksNamesExistInDb foundBooksIds = new BooksNamesExistInDb
            {
                BooksNamesIds = allBooksNamesPairings.Select(p => new BooksNamesSortByLanguageIdSortByBookId
                {
                    BookId = p.Key,
                    BooksDescriptions = p.OrderBy(s => s.LanguageId).Select(s => new BooksNamesSortByLanguageId { LanguageId = s.LanguageId, Sentence = s }).ToList()
                }
                ).ToList()
            };

            return foundBooksIds;
        }
        #endregion


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
                    Sentences = p.OrderBy(v => v.BookContentInChapter).Select(s => s).ToList()
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
                .ThenBy(si => si.BookContentInChapter)
                .ToList();

            // Set List to Redis
            string createdKeyNameFromRequest = where + ":" + whereValue.ToString(); //выдачу из базы сохранить как есть, с ключом bookId:(selected BookId)                                                                                
            await _access.SetObjectAsync(createdKeyNameFromRequest, requestedSelectResult, TimeSpan.FromDays(1));

            return requestedSelectResult;
        }

        #endregion
    }
}

