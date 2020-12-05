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
        private readonly IControllerDbManager _db;        
        private readonly IAccessCacheData _access;        

        public ControllerCacheManager(
            ILogger<ControllerDataManager> logger,
            IControllerDbManager db,            
            IAccessCacheData access)
        {
            _logger = logger;
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
    }
}
