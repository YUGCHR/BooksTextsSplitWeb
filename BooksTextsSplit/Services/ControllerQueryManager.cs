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
    public interface IControllerQueryManager
    {
        public Task<List<T>> GetDistinctBooksIdsList<T>(int languageId, int recordActualityLevel);
        public Task<List<T>> GetDistinctVersionsList<T>(int languageId, int recordActualityLeve, int currentBookId);
    }

    public class ControllerQueryManager : IControllerQueryManager
    {
        private readonly ILogger<ControllerDataManager> _logger;
        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;

        public ControllerQueryManager(
            ILogger<ControllerDataManager> logger,
            ICosmosDbService cosmosDbService,
            IAccessCacheData access)
        {
            _logger = logger;
            _access = access;
            _context = cosmosDbService;
        }


        public async Task<List<T>> GetDistinctBooksIdsList<T>(int languageId, int recordActualityLevel)
        {
            //SELECT DISTINCT VALUE c.bookId FROM c WHERE c.languageId = 1 AND c.recordActualityLevel = 5 (without VALUE - for additional control)
            string queryString = $"SELECT DISTINCT c.{Constants.FieldNameBooksId} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameRecordActualityLevel} = {recordActualityLevel}";
            return await _context.GetItemsListAsyncFromDb<T>(queryString);
        }

        public async Task<List<T>> GetDistinctVersionsList<T>(int languageId, int recordActualityLeve, int currentBookId)
        {
            //SELECT DISTINCT c.uploadVersion FROM c WHERE c.languageId = 1 AND c.recordActualityLevel = 5 AND c.bookId IN (77, 88, 39, 37)                                                                                             
            //SELECT DISTINCT VALUE c.uploadVersion FROM c where c.bookSentenceId = 1 AND c.languageId = 1 AND c.bookId = 77
            string queryString = $"SELECT DISTINCT c.{Constants.FieldNameUploadVersion} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameRecordActualityLevel} = {recordActualityLeve} AND c.bookId = {currentBookId}";

            return await _context.GetItemsListAsyncFromDb<T>(queryString);
        }
    }
}
