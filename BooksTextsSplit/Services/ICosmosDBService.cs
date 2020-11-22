using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using BooksTextsSplit.Models;

namespace BooksTextsSplit.Services
{
    public interface ICosmosDbService
    {
        Task<IEnumerable<TextSentence>> GetItemsAsync(string query);
        public Task<int?> GetCountItemAsync(string fieldName, int languageId);
        public Task<List<T>> GetItemsListAsync<T>(int languageId, int recordActualityLevel);
        public Task<List<T>> GetItemsListAsync<T>(int languageId, int recordActualityLeve, int currentBookId);
        public Task<T> GetItemCountAsync<T>(string queryString);
        Task<TextSentence> GetItemAsync(string id);
        Task AddItemAsync(TextSentence item);
        Task UpdateItemAsync(string id, TextSentence item);
        Task DeleteItemAsync(string id);
    }
}

