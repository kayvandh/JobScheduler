using Framework.Cache;
using Framework.Cache.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace JobScheduler.Infrastructure.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private const string CacheKeysKey = "__AllCacheKeys";

        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
            if (!_cache.TryGetValue(CacheKeysKey, out HashSet<string> _))
            {
                _cache.Set(CacheKeysKey, new HashSet<string>());
            }
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };
            _cache.Set(key, value, options);

            if (_cache.Get(CacheKeysKey) is HashSet<string> keys)
            {
                keys.Add(key);
            }
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<CacheOptions, Task<T>> func)
        {
            if (_cache.TryGetValue(key, out T? value))
                return value;

            var options = new CacheOptions();
            var result = await func(options);

            var memoryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.Expiration
            };
            _cache.Set(key, result, memoryOptions);

            if (_cache.Get(CacheKeysKey) is HashSet<string> keys)
            {
                keys.Add(key);
            }

            return result;
        }

        public async Task<(bool Found, T? Value)> TryGetValueAsync<T>(string key)
        {
            return await Task.Run(() =>
            {
                if (_cache.TryGetValue(key, out T? value))
                    return (true, value);

                return (false, default);
            });
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            if (_cache.Get(CacheKeysKey) is HashSet<string> keys)
            {
                var toRemove = keys.Where(k => k.StartsWith(prefix)).ToList();

                foreach (var key in toRemove)
                {
                    _cache.Remove(key);
                    keys.Remove(key);
                }
            }

            return Task.CompletedTask;
        }
    }
}