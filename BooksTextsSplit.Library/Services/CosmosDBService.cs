using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooksTextsSplit.Library.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace BooksTextsSplit.Library.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _container;
        private readonly Container _containerUser;
        private readonly ILogger<CosmosDbService> _logger;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName,
            string userContainerName,
            ILogger<CosmosDbService> logger)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
            this._containerUser = dbClient.GetContainer(databaseName, userContainerName);
            this._logger = logger;
            _logger.LogInformation("CosmosDbService started");
        }

        public async Task AddItemAsync(TextSentence item)
        {
            //ItemResponse<dynamic> itemResponse = await this._container.CreateItemAsync<dynamic>(item: new { id = item.Id, pk = item.Id, payload = item }, partitionKey: new PartitionKey(item.Id));
            ItemResponse<TextSentence> itemResponse = await this._container.CreateItemAsync<TextSentence>(item, new PartitionKey(item.Id));
            double requestCharge = itemResponse.RequestCharge;            
            string logString = $"{item.BookProperties.BookName.Trim()} - chapter {item.ChapterId} was uploaded with charge {requestCharge} RU";
            _logger.LogInformation(logString);
            // await this._container.CreateItemAsync<TextSentence>(item, new PartitionKey(item.Id));
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

        public async Task<int?> GetCountAllLanguageItemsAsync(string fieldName, int languageId)
        {
            string queryString = $"SELECT VALUE COUNT(1) FROM c WHERE c.{fieldName} = {languageId}";
            int result = default;
            try
            {
                FeedIterator<int> feedIterator = this._container.GetItemQueryIterator<int>(queryString);
                if (feedIterator.HasMoreResults)
                {
                    FeedResponse<int> feedResponse = await feedIterator.ReadNextAsync();

                    double requestCharge = feedResponse.RequestCharge; // request unit charge for operations executed in Cosmos DB 

                    foreach (var item in feedResponse)
                    {
                        result = item;
                    }
                }

                return result;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        //SELECT DISTINCT VALUE c.totalBookCounts.inBookChaptersCount FROM c where c.languageId = 1 AND c.recordActualityLevel = 5






        //public async Task<List<T>> GetItemsListAsync<T>(string fieldName1, int languageId)
        //{
        //    //SELECT VALUE COUNT(c.bookSentenceId) FROM c where c.languageId = 0 AND c.bookId = 77
        //    string queryString = $"SELECT VALUE COUNT(c.{Constants.FieldNameBookSentenceId}) FROM c WHERE c.{fieldName1} = {languageId} AND c.bookId = ";

        //    return await GetItemsListAsyncFromDb<T>(queryString);
        //}

        public async Task<List<T>> GetItemsListAsyncFromDb<T>(string queryString) //for BookId and UploadVersion
        {
            List<T> results = new List<T>();
            try
            {
                FeedIterator<T> feedIterator = this._container.GetItemQueryIterator<T>(queryString); // (new QueryDefinition(queryString))
                if (feedIterator.HasMoreResults)
                {
                    FeedResponse<T> feedResponse = await feedIterator.ReadNextAsync();

                    double requestCharge = feedResponse.RequestCharge; // request unit charge for operations executed in Cosmos DB 
                    // to add requestCharge in results
                    foreach (var item in feedResponse)
                    {
                        results.Add(item);
                    }
                }
                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("GetItemQueryIterator", ex);
                //_logger.LogInformation("CosmosException on query \n {queryString} \n" + ex.Message, queryString);
                return default;
            }
        }

        #region Get Users List

        public async Task<List<T>> GetUsersListAsync<T>(int userCount)
        {
            if (userCount != 0)
            {
                return default;
            }
            //SELECT * FROM c
            string queryString = $"SELECT * FROM c";
            return await GetUsersListAsyncFromDb<T>(queryString);
        }

        private async Task<List<T>> GetUsersListAsyncFromDb<T>(string queryString) // _containerUser
        {
            List<T> results = new List<T>();
            try
            {
                FeedIterator<T> feedIterator = this._containerUser.GetItemQueryIterator<T>(queryString); // (new QueryDefinition(queryString))
                if (feedIterator.HasMoreResults)
                {
                    FeedResponse<T> feedResponse = await feedIterator.ReadNextAsync();

                    double requestCharge = feedResponse.RequestCharge; // request unit charge for operations executed in Cosmos DB 

                    foreach (var item in feedResponse)
                    {
                        results.Add(item);
                    }
                }
                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("GetItemQueryIterator", ex);
                //_logger.LogInformation("CosmosException on query \n {queryString} \n" + ex.Message, queryString);
                return default;
            }
        }

        #endregion

        public async Task<IEnumerable<TextSentence>> GetItemsAsync(string queryString)
        {// move all references to GetUsersListAsyncFromDb
            var query = this._container.GetItemQueryIterator<TextSentence>(new QueryDefinition(queryString));
            List<TextSentence> results = new List<TextSentence>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
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




        public async Task UpdateItemAsync(string id, TextSentence item)
        {
            await this._container.UpsertItemAsync<TextSentence>(item, new PartitionKey(id));
        }
    }
}
