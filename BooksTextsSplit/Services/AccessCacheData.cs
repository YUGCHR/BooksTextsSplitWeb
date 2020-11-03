using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooksTextsSplit.Models;
using BooksTextsSplit.Services;
using CachingFramework.Redis;
using Microsoft.Extensions.Logging;

namespace BooksTextsSplit.Services
{
    public interface IAccessCacheData
    {
        public Task<T> GetObjectAsync<T>(string key);
        public Task<T> FetchObjectAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null);
        public Task SetObjectAsync<T>(string key, T value, TimeSpan? ttl = null);
        public Task<bool> SetObjectAsyncCheck<T>(string key, T value, TimeSpan? ttl = null);
        public Task<bool> RemoveAsync(string key);
        public Task<bool> KeyExistsAsync(string key);
        public Task<bool> KeyExpireAsync(string key, DateTime expiration);

    }
    public class AccessCacheData : IAccessCacheData
    {
        private readonly ILogger<AccessCacheData> _logger;
        private readonly RedisContext cache;
        private readonly ICosmosDbService _context;

        public AccessCacheData(
            ILogger<AccessCacheData> logger,
            ICosmosDbService cosmosDbService,
            RedisContext c)
        {
            _logger = logger;
            cache = c;
            _context = cosmosDbService;
        }

        public async Task<T> GetObjectAsync<T>(string key)
        {
            _logger.LogInformation($"AccessCacheData Service started GetObjectAsync with key = {key}");
            var cacheValue = await cache.Cache.GetObjectAsync<T>(key);
            if (cacheValue != null)
            {
                return cacheValue;
            }
            return default;

        }

        public async Task<T> FetchObjectAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null)
        {
            T value = default;
            var cacheValue = await cache.Cache.GetObjectAsync<T>(key);
            if (cacheValue != null)
            {
                value = cacheValue;
            }
            else
            {
                var task = func.Invoke();
                if (task != null)
                {
                    value = await task;
                    if (value != null)
                    {
                        await cache.Cache.SetObjectAsync(key, value, expiry);
                    }
                }
            }
            return value;
        }

        public async Task SetObjectAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            await cache.Cache.SetObjectAsync(key, value, ttl);
        }

        public async Task<bool> SetObjectAsyncCheck<T>(string key, T value, TimeSpan? ttl = null)
        {
            await cache.Cache.SetObjectAsync(key, value, ttl);
            return await cache.Cache.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await cache.Cache.RemoveAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await cache.Cache.KeyExistsAsync(key);
        }

        public async Task<bool> KeyExpireAsync(string key, DateTime expiration) //Set a timeout on key. After the timeout has expired, the key will automatically be deleted
        {
            return await cache.Cache.KeyExpireAsync(key, expiration);
        }

        //public async Task<TimeSpan?> KeyTimeToLiveAsync(string key)
        //public async Task<bool> KeyPersistAsync(string key)
        //public async Task FlushAllAsync()
    }
}
