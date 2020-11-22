using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BooksTextsSplit.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BooksTextsSplit.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;
        //private readonly ILogger<ControllerDataManager> _logger;

        public CosmosDbService(
            //ILogger<ControllerDataManager> logger,
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            //_logger = logger;
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(TextSentence item)
        {
            await this._container.CreateItemAsync<TextSentence>(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<TextSentence>(id, new PartitionKey(id));
        }

        public async Task<TextSentence> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<TextSentence> response = await this._container.ReadItemAsync<TextSentence>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<int?> GetCountItemAsync(string fieldName, int languageId)
        {
            string id = $"SELECT VALUE COUNT(1) FROM c WHERE c.{fieldName} = {languageId}";
            try
            {
                ItemResponse<int> response = await this._container.ReadItemAsync<int>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        //SELECT DISTINCT VALUE c.totalBookCounts.inBookChaptersCount FROM c where c.languageId = 1 AND c.recordActualityLevel = 5


        public async Task<List<T>> GetItemsListAsync<T>(int languageId, int recordActualityLevel)
        {
            //SELECT DISTINCT VALUE c.bookId FROM c WHERE c.languageId = 1 AND c.recordActualityLevel = 5 (without VALUE - for additional control)
            string queryString = $"SELECT DISTINCT c.{Constants.FieldNameBooksId} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameRecordActualityLevel} = {recordActualityLevel}";
            return await GetItemsListAsyncFromDb<T>(queryString);
        }

        public async Task<List<T>> GetItemsListAsync<T>(int languageId, int recordActualityLeve, int currentBookId)
        {
            //SELECT DISTINCT c.uploadVersion FROM c WHERE c.languageId = 1 AND c.recordActualityLevel = 5 AND c.bookId IN (77, 88, 39, 37)                                                                                             
            //SELECT DISTINCT VALUE c.uploadVersion FROM c where c.bookSentenceId = 1 AND c.languageId = 1 AND c.bookId = 77
            string queryString = $"SELECT DISTINCT c.{Constants.FieldNameUploadVersion} FROM c WHERE c.{Constants.FieldNameLanguageId} = {languageId} AND c.{Constants.FieldNameRecordActualityLevel} = {recordActualityLeve} AND c.bookId = {currentBookId}";

            return await GetItemsListAsyncFromDb<T>(queryString);
        }

        //public async Task<List<T>> GetItemsListAsync<T>(string fieldName1, int languageId)
        //{
        //    //SELECT VALUE COUNT(c.bookSentenceId) FROM c where c.languageId = 0 AND c.bookId = 77
        //    string queryString = $"SELECT VALUE COUNT(c.{Constants.FieldNameBookSentenceId}) FROM c WHERE c.{fieldName1} = {languageId} AND c.bookId = ";

        //    return await GetItemsListAsyncFromDb<T>(queryString);
        //}

        private async Task<List<T>> GetItemsListAsyncFromDb<T>(string queryString)
        {
            List<T> distinctBooksIds = new List<T>();
            try
            {
                FeedIterator<T> feedIterator = this._container.GetItemQueryIterator<T>(queryString);
                if (feedIterator.HasMoreResults)
                {
                    FeedResponse<T> feedResponse = await feedIterator.ReadNextAsync();
                                            
                    double requestCharge = feedResponse.RequestCharge; // request unit charge for operations executed in Cosmos DB 

                    foreach (var item in feedResponse)
                    {
                        distinctBooksIds.Add(item);
                    }
                }                
                
                return distinctBooksIds;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("GetItemQueryIterator", ex);
                //_logger.LogInformation("CosmosException on query \n {queryString} \n" + ex.Message, queryString);
                return default;
            }
        }





        public async Task<T> GetItemCountAsync<T>(string queryString)
        {
            T result = default;
            try
            {
                FeedIterator<T> feedIterator = this._container.GetItemQueryIterator<T>(queryString);
                if (feedIterator.HasMoreResults)
                {
                    FeedResponse<T> feedResponse = await feedIterator.ReadNextAsync();

                    double requestCharge = feedResponse.RequestCharge; // request unit charge for operations executed in Cosmos DB 

                    foreach (var item in feedResponse)
                    {
                        result = item;
                    }
                }

                return result;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("GetItemQueryIterator", ex);
                //_logger.LogInformation("CosmosException on query \n {queryString} \n" + ex.Message, queryString);
                return default;
            }
        }


        public async Task<IEnumerable<TextSentence>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<TextSentence>(new QueryDefinition(queryString));
            List<TextSentence> results = new List<TextSentence>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, TextSentence item)
        {
            await this._container.UpsertItemAsync<TextSentence>(item, new PartitionKey(id));
        }
    }
}
