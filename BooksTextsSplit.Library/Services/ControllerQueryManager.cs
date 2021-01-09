using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BooksTextsSplit.Library.Services
{
    public interface IControllerQueryManager
    {
        public Task<List<T>> GetDistinctBooksIdsList<T>(int languageId, int recordActualityLevel);
        public Task<List<T>> GetDistinctVersionsList<T>(int languageId, int recordActualityLeve, int currentBookId);
        public Task<List<T>> GetBooksNamesVersionsPropertiesFromDb<T>(int level);
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

        public async Task<List<T>> GetBooksNamesVersionsPropertiesFromDb<T>(int level)
        {
            // SELECT c.bookId, c.languageId, c.bookProperties, c.uploadVersion FROM c where c.recordActualityLevel = 6 AND c.recordId = 0
            string queryString = $"SELECT c.bookId, c.languageId, c.bookProperties, c.totalBookCounts, c.uploadVersion FROM c WHERE c.recordActualityLevel = {level} AND c.recordId = 0";

            return await _context.GetItemsListAsyncFromDb<T>(queryString);
        }

        public async Task<List<T>> GetBookNameVersionsPropertiesInLanguageFromDb<T>(int level, int bookId, int languageId)
        {
            // SELECT c.uploadVersion, c.bookProperties, c.totalBookCounts FROM c where 
            // c.recordActualityLevel = 6 AND c.recordId = 0 AND
            // c.bookId = 74 AND c.languageId = 0
            string queryString = $"SELECT c.bookId, c.languageId, c.bookProperties, c.totalBookCounts, c.uploadVersion FROM c WHERE c.recordActualityLevel = {level} AND c.recordId = 0";

            return await _context.GetItemsListAsyncFromDb<T>(queryString);
        }
    }
}
