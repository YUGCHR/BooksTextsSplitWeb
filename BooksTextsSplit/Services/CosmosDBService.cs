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

        public async Task<int?> GetCountItemAsync(string id)
        {
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

        

        public async Task<List<T>> GetItemListAsync<T>(string queryString)
        {
            List<T> distinctBooksIds = new List<T>();
            try
            {
                FeedIterator<T> feedIterator = this._container.GetItemQueryIterator<T>(queryString);
                if (feedIterator.HasMoreResults)
                {
                    // request unit charge for operations executed in Cosmos DB
                    FeedResponse<T> feedResponse = await feedIterator.ReadNextAsync();
                    double requestCharge = feedResponse.RequestCharge;

                    foreach (var item in await feedIterator.ReadNextAsync())
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
