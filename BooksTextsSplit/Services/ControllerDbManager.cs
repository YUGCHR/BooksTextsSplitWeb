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
        public Task<List<BookPropertiesExistInDb>> FetchBooksNamesVersionsPropertiesFromDb(int level);
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

        public async Task<List<BookPropertiesExistInDb>> FetchBooksNamesVersionsPropertiesFromDb(int level) // передавать список требуемых полей из базы?
        {
            List<TextSentence> booksVersionsProperties = await _query.GetBooksNamesVersionsPropertiesFromDb<TextSentence>(level);

            List<BookPropertiesExistInDb> foundBooksIds = booksVersionsProperties.GroupBy(r => r.BookId).Select(b => new BookPropertiesExistInDb
            {
                BookId = b.Key,
                BookVersionsLanguageInBook = b.GroupBy(l => l.LanguageId).Select(v => new BookVersionPropertiesInLanguage
                {
                    LanguageId = v.Key,
                    BookVersionsInLanguage = v.Select(t => new BookVersionProperties
                    {
                        UploadVersion = t.UploadVersion,
                        BookDescriptionDetails = t.BookProperties,
                        BookVersionCounts = t.TotalBookCounts
                    }).ToList()
                }).ToList()
            }).ToList();

            return foundBooksIds;
        }
    }
}
