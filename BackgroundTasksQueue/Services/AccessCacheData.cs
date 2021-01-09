using System;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;

namespace BackgroundTasksQueue.Services
{
    public interface IAccessCacheData
    {
        public Task<T> GetObjectAsync<T>(string key);
        public Task<T> GetObjectAsync<T>(string key, string field);
        public Task InsertUser<T>(T user, string userId);
        public Task<T> FetchObjectAsync<T>(string redisKey, string fieldKey, Func<Task<T>> func, TimeSpan? expiry = null); // FetchHashedAsync
        public Task<T> FetchObjectAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null);
        public Task SetObjectAsync<T>(string key, T value, TimeSpan? ttl = null);
        public Task SetObjectAsync<T>(string redisKey, string fieldKey, T value, TimeSpan? ttl = null); // SetHashedAsync
        public Task<bool> SetObjectAsyncCheck<T>(string key, T value, TimeSpan? ttl = null);
        public Task<bool> RemoveAsync(string key);
        public Task<bool> KeyExistsAsync(string key);
        public Task<bool> KeyExistsAsync<T>(string key, string field);
        public Task<bool> KeyExpireAsync(string key, DateTime expiration);

    }
    public class AccessCacheData : IAccessCacheData
    {
        private readonly ILogger<AccessCacheData> _logger;
        private readonly ISettingConstants _constant;
        private readonly ICacheProviderAsync _cache;
        private readonly ICosmosDbService _context;

        public AccessCacheData(
            ILogger<AccessCacheData> logger,
            ISettingConstants constant,
            ICosmosDbService cosmosDbService,
            //RedisContext c)
            ICacheProviderAsync cache)
        {
            _logger = logger;
            _constant = constant;
            _cache = cache;
            _context = cosmosDbService;
        }

        public async Task<T> GetObjectAsync<T>(string key)
        {
            var cacheValue = await _cache.GetObjectAsync<T>(key);
            if (cacheValue != null)
            {
                return cacheValue;
            }
            return default;
        }
        
        public async Task<T> GetObjectAsync<T>(string key, string field)
        {
            var cacheValue = await _cache.GetHashedAsync<T>(key, field);
            if (cacheValue != null)
            {
                return cacheValue;
            }
            return default;
        }

        public async Task InsertUser<T>(T user, string userId) // for GetTest() only
        {
            var redisKey = "users:added";
            var fieldKey = $"user:id:{userId}";
            await _cache.SetHashedAsync<T>(redisKey, fieldKey, user);
        }

        public async Task<T> FetchObjectAsync<T>(string redisKey, string fieldKey, Func<Task<T>> func, TimeSpan? expiry = null)
        {
            return await _cache.FetchHashedAsync<T>(redisKey, fieldKey, func, expiry);            
        }

        public async Task<T> FetchObjectAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null)
        {
            //var cacheValue = await _cache.GetObjectAsync<T>(key);
            if (await _cache.KeyExistsAsync(key))
            {
                return await _cache.GetObjectAsync<T>(key);
            }
            else
            {
                T value = default;
                var task = func.Invoke();
                if (task != null)
                {
                    value = await task;
                    if (value != null)
                    {
                        await _cache.SetObjectAsync(key, value, expiry);
                    }
                }
                return value;
            }
        }

        public async Task SetObjectAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            await _cache.SetObjectAsync(key, value, ttl);
        }

        public async Task SetObjectAsync<T>(string redisKey, string fieldKey, T value, TimeSpan? ttl = null)
        {
            //ttl ??= TimeSpan.FromMinutes(_constant.GetPercentsKeysExistingTimeInMinutes);
            await _cache.SetHashedAsync<T>(redisKey, fieldKey, value, ttl);
        }

        public async Task<bool> SetObjectAsyncCheck<T>(string key, T value, TimeSpan? ttl = null)
        {
            await _cache.SetObjectAsync(key, value, ttl);
            return await _cache.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _cache.RemoveAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _cache.KeyExistsAsync(key);
        }

        public async Task<bool> KeyExistsAsync<T>(string key, string field)
        {
            var cacheValue = await _cache.GetHashedAsync<T>(key, field);
            if (cacheValue != null)
            {
                return true;
            }
            return false;
        }        

        public async Task<bool> KeyExpireAsync(string key, DateTime expiration) //Set a timeout on key. After the timeout has expired, the key will automatically be deleted
        {
            return await _cache.KeyExpireAsync(key, expiration);
        }

        //public async Task<TimeSpan?> KeyTimeToLiveAsync(string key)
        //public async Task<bool> KeyPersistAsync(string key)
        //public async Task FlushAllAsync()
    }
}
