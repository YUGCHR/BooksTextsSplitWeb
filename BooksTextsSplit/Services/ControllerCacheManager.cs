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
    public interface IControllerCacheManager
    {
        public Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel);
        public Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel, int currentBooksIds);
        public Task<List<BookPropertiesExistInDb>> FetchAllBookIdsLanguageIdsFromCache();
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
            
            string keyBookId = "bookId";
            string keyBookIdNum = "all";
            var redisKey = $"{keyBookId}:{keyBookIdNum}";
            string keyLanguageId = "languageId";
            string keyLanguageIdNum = "all";
            var fieldKey = $"{keyLanguageId}:{keyLanguageIdNum}";

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
    }
}
