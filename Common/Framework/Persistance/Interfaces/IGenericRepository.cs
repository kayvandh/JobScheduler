using System.Linq.Expressions;

namespace Framework.Persistance.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

        Task<ListResult<T>> GetAllAsync(
            PageRequest? pageRequest = null,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);

        Task<ListResult<T>> GetAsync(
            Expression<Func<T, bool>> predicate,
            PageRequest? pageRequest = null,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);

        Task<ListResult<T>> GetAsync(
            ISpecification<T> specification,
            PageRequest? pageRequest = null,
            CancellationToken cancellationToken = default);

        Task<T?> GetOneAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);

        Task<IQueryable<T>> GetQueryableAsync(
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);

        Task<bool> HasAnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task<decimal> SumAsync(
            Expression<Func<T, decimal>> selector,
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task<decimal> AvgAsync(
            Expression<Func<T, decimal>> selector,
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        void Update(T entity);

        void Delete(T entity);

        void Delete(IEnumerable<T> entities);

        Task<bool> DeleteByIdAsync(object id);
    }
}