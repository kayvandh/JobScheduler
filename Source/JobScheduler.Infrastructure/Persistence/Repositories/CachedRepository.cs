using Framework.Cache.Interface;
using Framework.Persistance;
using Framework.Persistance.Interfaces;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace JobScheduler.Infrastructure.Persistence.Repositories
{
    public class CachedRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly IGenericRepository<T> _innerRepository;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheExpiration;
        private readonly string _cachePrefix;

        public CachedRepository(
            IGenericRepository<T> innerRepository,
            ICacheService cacheService,
            TimeSpan? cacheExpiration = null)
        {
            _innerRepository = innerRepository;
            _cacheService = cacheService;
            _cacheExpiration = cacheExpiration ?? TimeSpan.FromMinutes(5);
            _cachePrefix = typeof(T).FullName ?? typeof(T).Name;
        }

        public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            var key = BuildCacheKey("ById", id?.ToString() ?? string.Empty);
            return await _cacheService.GetOrCreateAsync(key, async _ =>
            {
                return await _innerRepository.GetByIdAsync(id!, cancellationToken);
            });
        }

        public async Task<ListResult<T>> GetAllAsync(
            PageRequest? pageRequest = null,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            string key = BuildCacheKey(
                "GetAll",
                pageRequest?.PageNumber.ToString() ?? "0",
                pageRequest?.PageSize.ToString() ?? "0",
                includes?.Select(ExpressionToString).Aggregate((a, b) => a + "|" + b) ?? "",
                orderBy?.Method.Name ?? "null"
            );

            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.GetAllAsync(pageRequest, cancellationToken, orderBy, includes ?? Array.Empty<Expression<Func<T, object>>>());
            });
        }

        public async Task<ListResult<T>> GetAsync(Expression<Func<T, bool>> predicate, PageRequest? pageRequest = null, CancellationToken cancellationToken = default, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, params Expression<Func<T, object>>[] includes)
        {
            string key = BuildCacheKey(
                "Find",
                ExpressionToString(predicate),
                pageRequest?.PageNumber.ToString() ?? "0",
                pageRequest?.PageSize.ToString() ?? "0",
                includes?.Select(ExpressionToString).Aggregate((a, b) => a + "|" + b) ?? "",
                orderBy?.Method.Name ?? "null"
            );

            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.GetAsync(predicate, pageRequest, cancellationToken, orderBy, includes ?? Array.Empty<Expression<Func<T, object>>>());
            });
        }

        public async Task<ListResult<T>> GetAsync(ISpecification<T> specification, PageRequest? pageRequest = null, CancellationToken cancellationToken = default)
        {
            string key = BuildCacheKey(
                "FindSpec",
                specification?.ToString() ?? "",
                pageRequest?.PageNumber.ToString() ?? "0",
                pageRequest?.PageSize.ToString() ?? "0"
            );

            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.GetAsync(specification!, pageRequest, cancellationToken);
            });
        }

        public async Task<bool> HasAnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            string key = BuildCacheKey("Count", predicate != null ? ExpressionToString(predicate) : string.Empty);
            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.HasAnyAsync(predicate!, cancellationToken);
            });
        }

        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            string key = BuildCacheKey("GetOne", ExpressionToString(predicate));
            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.GetOneAsync(predicate, cancellationToken, includes);
            });
        }

        public async Task<IQueryable<T>> GetQueryableAsync(
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            return await _innerRepository.GetQueryableAsync(cancellationToken, includes);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            string key = BuildCacheKey("Count", predicate != null ? ExpressionToString(predicate) : string.Empty);
            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.CountAsync(predicate, cancellationToken);
            });
        }

        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            string key = BuildCacheKey("Sum", ExpressionToString(selector), predicate != null ? ExpressionToString(predicate) : string.Empty);
            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.SumAsync(selector, predicate, cancellationToken);
            });
        }

        public async Task<decimal> AvgAsync(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            string key = BuildCacheKey("Avg", ExpressionToString(selector), predicate != null ? ExpressionToString(predicate) : string.Empty);
            return await _cacheService.GetOrCreateAsync(key, async (option) =>
            {
                option.Expiration = _cacheExpiration;
                return await _innerRepository.AvgAsync(selector, predicate, cancellationToken);
            });
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _innerRepository.AddAsync(entity, cancellationToken);
            await InvalidateCacheAsync();
        }

        public void Update(T entity)
        {
            _innerRepository.Update(entity);
            _ = InvalidateCacheAsync();
        }

        public void Delete(T entity)
        {
            _innerRepository.Delete(entity);
            _ = InvalidateCacheAsync();
        }

        public void Delete(IEnumerable<T> entities)
        {
            _innerRepository.Delete(entities);
            _ = InvalidateCacheAsync(); ;
        }

        public async Task<bool> DeleteByIdAsync(object id)
        {
            var result = await _innerRepository.DeleteByIdAsync(id);
            if (result) await InvalidateCacheAsync();
            return result;
        }

        //-----------------------------------

        private string ExpressionToString(Expression expression) => expression?.ToString() ?? "null";

        private string BuildCacheKey(params string[] parts)
        {
            var rawKey = $"{_cachePrefix}:{string.Join(":", parts)}";
            var hash = ComputeSha1(rawKey);
            return $"{_cachePrefix}:{hash}";
        }

        private static string ComputeSha1(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha1.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private async Task InvalidateCacheAsync()
        {
            await _cacheService.RemoveByPrefixAsync(_cachePrefix);
        }
    }
}