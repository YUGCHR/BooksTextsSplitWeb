using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BooksTextsSplit.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BooksTextsSplit.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
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

        public class BooksIdsList
        {
            public int bookId { get; set; }
        }

        public class BookIdValue
        {
            public int bookId { get; set; }
        }

        public async Task<T> GetJsonItemAsync<T>(string queryString)
        {
            try
            {
                //var iterator = this._container.GetItemQueryIterator<string>(new QueryDefinition(queryString));
                FeedIterator<BooksIdsList> feedIterator = this._container.GetItemQueryIterator<BooksIdsList>(queryString);

                while (feedIterator.HasMoreResults)
                {
                    //FeedResponse<BooksIdsList> currentResultSet = await feedIterator.ReadNextAsync();
                    foreach (var item in await feedIterator.ReadNextAsync())
                    {
                        Console.WriteLine(item.bookId);
                    }

                    return default;
                }
                return default;
            }
            catch (CosmosException ex)
            {
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
