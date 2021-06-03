using BooksTextsSplit.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BooksTextsSplit.Library.Helpers;

namespace BooksTextsSplit.Library.Services
{
    public interface IControllerCacheManager
    {
        public Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel);
        public Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel, int currentBooksIds);
        public Task<List<BookPropertiesExistInDb>> FetchAllBookIdsLanguageIdsFromCache();
        public Task<TaskUploadPercents> FetchTaskState(TaskUploadPercents taskStateCurrent);
        public Task<bool> SetTaskGuidKeys(TaskUploadPercents uploadPercents, int keysExistingTimeFactor);
        public Task<BookTable> CheckBookId(string bookTablesKey, int bookId);
        public Task AddHashValue<T>(string Key, int id, T context);
    }

    public class ControllerCacheManager : IControllerCacheManager
    {
        private readonly ILogger<ControllerDataManager> _logger;
        private readonly ISettingConstants _constant;
        private readonly IControllerDbManager _db;
        private readonly IAccessCacheData _access;

        public ControllerCacheManager(
            ILogger<ControllerDataManager> logger,
            ISettingConstants constant,
            IControllerDbManager db,
            IAccessCacheData access)
        {
            _logger = logger;
            _constant = constant;
            _db = db;
            _access = access;
        }

        public async Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel)
        {
            int[] allBooksIds = await _access.FetchObjectAsync<int[]>(keyBooksIds, () => _db.FetchItemsArrayFromDb(languageId, propName, actualityLevel));

            return allBooksIds;
        }

        public async Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel, int currentBooksIds)
        {
            int[] allBooksIds = await _access.FetchObjectAsync<int[]>(keyBooksIds, () => _db.FetchItemsArrayFromDb(languageId, propName, actualityLevel, currentBooksIds));

            return allBooksIds;
        }

        public async Task<List<BookPropertiesExistInDb>> FetchAllBookIdsLanguageIdsFromCache()
        {
            // определиться, откуда взять recordActualityLevel (from Constant or from UI - and UI will receive from Constant)
            int level = _constant.GetRecordActualityLevel; // Constants.RecordActualityLevel;

            string keyBookId = _constant.GetKeyBookId; // bookId
            string keyBookIdNum = _constant.GetKeyAllNumbers; // all
            string redisKey = keyBookId.KeyBaseRedisKey(keyBookIdNum); // bookId:all
            string keyLanguageId = _constant.GetKeyLanguageId; // languageId
            string keyLanguageIdNum = _constant.GetKeyAllNumbers; // all
            string fieldKey = keyLanguageId.KeyBaseRedisKey(keyLanguageIdNum); // languageId:all

            List<BookPropertiesExistInDb> foundBooksIds = await _access.FetchObjectAsync<List<BookPropertiesExistInDb>>(redisKey, fieldKey, () => _db.FetchBooksNamesVersionsPropertiesFromDb(level));

            return foundBooksIds;
        }

        public async Task<List<T>> FetchBookIdLanguageIdFromCache<T>()
        {
            int level = _constant.GetRecordActualityLevel;
            List<BookPropertiesExistInDb> foundBooksIds = await _db.FetchBooksNamesVersionsPropertiesFromDb(level);

            for (int i = 0; i < foundBooksIds.Count; i++)
            {
                string keyBookId = "bookId";
                string keyBookIdNum = foundBooksIds[i].BookId.ToString();
                var redisKey = $"{keyBookId}:{keyBookIdNum}";

                for (int j = 0; j < 2; j++)
                {
                    var lang = foundBooksIds[i].BookVersionsLanguageInBook;

                    string keyLanguageId = "languageId";
                    string keyLanguageIdNum = lang[j].LanguageId.ToString();
                    var fieldKey = $"{keyLanguageId}:{keyLanguageIdNum}";



                    //var booksVersionsProperties = await _access.FetchObjectAsync<List<T>>(redisKey, fieldKey, () => ());
                }
            }
            return default;
        }

        public async Task<TaskUploadPercents> FetchTaskState(TaskUploadPercents taskStateCurrent)
        {
            return await _access.GetObjectAsync<TaskUploadPercents>(taskStateCurrent.RedisKey, taskStateCurrent.FieldKeyPercents); 
        }

        public async Task<bool> SetTaskGuidKeys(TaskUploadPercents uploadPercents, int keysExistingTimeFactor)
        {
            TimeSpan keysExistingTime = TimeSpan.FromMinutes(_constant.GetPercentsKeysExistingTimeInMinutes) * keysExistingTimeFactor;
            await _access.SetObjectAsync(uploadPercents.RedisKey, uploadPercents.FieldKeyPercents, uploadPercents, keysExistingTime);
            return true;
        }

        public async Task<BookTable> CheckBookId(string bookTablesKey, int bookId)
        {
            IDictionary<int, BookTable> existedBookIds = await _access.FetchAndCheckBookId<int, BookTable>(bookTablesKey);

            if(existedBookIds == null)
            {
                return null;
            }

            foreach (var b in existedBookIds)
            {
                var (bookNumber, bookTable) = b;

                if(bookNumber == bookId)
                {
                    return bookTable;
                }

            }
                return null;
        }

        public async Task AddHashValue<T>(string Key, int id, T context)
        {
            double chaptersExistingTime = 0.01; // время хранения книг в кэше, взять из констант
            
            await _access.WriteHashedAsync<int, T>(Key, id, context, chaptersExistingTime);
        }                
    }
}
