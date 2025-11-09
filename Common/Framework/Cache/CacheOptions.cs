namespace Framework.Cache
{
    public class CacheOptions
    {
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(30);
    }
}