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
    public class CosmosUserDbService : ICosmosUserDbService
    {
        private Container _container;        

        public CosmosUserDbService(
            //ILogger<ControllerDataManager> logger,
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            //_logger = logger;
            this._container = dbClient.GetContainer(databaseName, containerName);
        }


    }
}
