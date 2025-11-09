using Framework.Cache;
using Framework.Cache.Interface;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace JobScheduler.Infrastructure.Cache
{
    public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis) : ICacheService
    {
        private readonly IDistributedCache _cache = cache;
        private readonly IConnectionMultiplexer _redis = redis;

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            return data is null ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        public Task RemoveAsync(string key)
        {
            return _cache.RemoveAsync(key);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<CacheOptions, Task<T>> func)
        {
            var cached = await _cache.GetStringAsync(key);
            if (cached != null)
                return JsonSerializer.Deserialize<T>(cached)!;

            var options = new CacheOptions();
            var result = await func(options);

            var json = JsonSerializer.Serialize(result);

            var redisOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.Expiration
            };

            await _cache.SetStringAsync(key, json, redisOptions);
            return result;
        }

        public async Task<(bool Found, T? Value)> TryGetValueAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (data is null)
                return (false, default);

            var value = JsonSerializer.Deserialize<T>(data);
            return (true, value);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());

            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key);
            }
        }
    }
}