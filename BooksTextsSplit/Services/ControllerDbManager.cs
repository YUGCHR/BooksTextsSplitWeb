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
    public interface IControllerDbManager
    {
        public Task<int[]> FetchItemsArrayFromDb(int languageId, string propName, int recordActualityLevel);
        public Task<int[]> FetchItemsArrayFromDb(int languageId, string propName, int recordActualityLevel, int currentBooksIds);
        public Task<List<T>> FetchBooksNamesVersionsPropertiesFromDb<T>(int level);
    }

    public class ControllerDbManager : IControllerDbManager
    {
        private readonly ILogger<ControllerDataManager> _logger;        
        private readonly IControllerQueryManager _query;

        public ControllerDbManager(
            ILogger<ControllerDataManager> logger,            
            IControllerQueryManager query)
        {
            _logger = logger;            
            _query = query;            
        }

        public async Task<int[]> FetchItemsArrayFromDb(int languageId, string propName, int recordActualityLevel)
        {
            List<TextSentence> allBooksIds = await _query.GetDistinctBooksIdsList<TextSentence>(languageId, recordActualityLevel);
            var result = allBooksIds.Select(a => (int)a.GetType().GetProperty(propName).GetValue(a, null)).ToArray();
            return result;
        }

        public async Task<int[]> FetchItemsArrayFromDb(int languageId, string propName, int recordActualityLevel, int currentBooksIds)
        {
            List<TextSentence> allUploadedVersions = await _query.GetDistinctVersionsList<TextSentence>(languageId, recordActualityLevel, currentBooksIds);
            int[] uploadedVersions = (allUploadedVersions.Select(a => (int)a.GetType().GetProperty(propName).GetValue(a, null)).ToArray());

            return uploadedVersions;
        }

        public async Task<List<T>> FetchBooksNamesVersionsPropertiesFromDb<T>(int level)
        {
            var booksVersionsProperties = await _query.GetBooksNamesVersionsPropertiesFromDb<T>(level);
            
            return booksVersionsProperties;
        }
    }
}
