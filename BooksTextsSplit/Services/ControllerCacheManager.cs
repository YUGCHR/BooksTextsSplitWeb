using BooksTextsSplit.Models;
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
    }

    public class ControllerCacheManager : IControllerCacheManager
    {
        private readonly ILogger<ControllerDataManager> _logger;
        private readonly IControllerDbManager _query;
        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;

        public ControllerCacheManager(
            ILogger<ControllerDataManager> logger,
            IControllerDbManager query,
            ICosmosDbService cosmosDbService,
            IAccessCacheData access)
        {
            _logger = logger;
            _query = query;
            _access = access;
            _context = cosmosDbService;
        }

        public async Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel)
        {
            int[] allBooksIds = await _access.FetchObjectAsync<int[]>(keyBooksIds, () => FetchItemsArrayFromDb(languageId, propName, actualityLevel));

            return allBooksIds;
        }

        public async Task<int[]> FetchItemsArrayFromDb(int languageId, string propName, int recordActualityLevel) // always fetch data from db as version of book of language
        {
            List<TextSentence> allBooksIds = await _query.GetDistinctBooksIdsList<TextSentence>(languageId, recordActualityLevel);
            var result = allBooksIds.Select(a => (int)a.GetType().GetProperty(propName).GetValue(a, null)).ToArray();
            return result;
        }

        public async Task<int[]> FetchAllBooksIds(string keyBooksIds, int languageId, string propName, int actualityLevel, int currentBooksIds)
        {
            int[] allBooksIds = await _access.FetchObjectAsync<int[]>(keyBooksIds, () => FetchItemsArrayFromDb(languageId, propName, actualityLevel, currentBooksIds));

            return allBooksIds;
        }

        public async Task<int[]> FetchItemsArrayFromDb(int languageId, string propName, int recordActualityLevel, int currentBooksIds) // always fetch data from db as version of book of language
        {
            List<TextSentence> allUploadedVersions = await _context.GetDistinctVersionsList<TextSentence>(languageId, recordActualityLevel, currentBooksIds);
            int[] uploadedVersions = (allUploadedVersions.Select(a => (int)a.GetType().GetProperty(propName).GetValue(a, null)).ToArray());

            return uploadedVersions;
        }
    }
}
