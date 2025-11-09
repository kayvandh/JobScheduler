namespace Framework.Cache.Interface
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        Task RemoveAsync(string key);

        Task<T> GetOrCreateAsync<T>(string key, Func<CacheOptions, Task<T>> func);

        Task<(bool Found, T? Value)> TryGetValueAsync<T>(string key);

        Task RemoveByPrefixAsync(string prefix);
    }
}