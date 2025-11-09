using Framework.Persistance.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Framework.Persistance
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbSet<T> dbSet;
        private readonly DbContext dbContext;

        public GenericRepository(DbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
        }

        public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<ListResult<T>> GetAllAsync(
              PageRequest? pageRequest = null,
              CancellationToken cancellationToken = default,
              Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
              params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet.AsQueryable();

            query = ApplyIncludes(query, includes);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            if (pageRequest != null)
            {
                query = query.Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
                             .Take(pageRequest.PageSize);
            }

            var items = await query.ToListAsync(cancellationToken);

            return new ListResult<T>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<ListResult<T>> GetAsync(
            Expression<Func<T, bool>> predicate,
            PageRequest? pageRequest = null,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            var query = dbSet.AsQueryable();

            query = query.Where(predicate);

            query = ApplyIncludes(query, includes);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            if (pageRequest != null)
            {
                query = query.Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
                             .Take(pageRequest.PageSize);
            }

            var items = await query.ToListAsync(cancellationToken);

            return new ListResult<T>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<ListResult<T>> GetAsync(
            ISpecification<T> specification,
            PageRequest? pageRequest = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = dbSet.AsQueryable();

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            query = ApplyIncludes(query, specification.Includes.ToArray());

            if (specification.OrderBy != null)
            {
                query = specification.OrderBy(query);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            if (pageRequest != null)
            {
                query = query.Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
                             .Take(pageRequest.PageSize);
            }

            var items = await query.ToListAsync(cancellationToken);

            return new ListResult<T>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet.AsQueryable();

            query = query.Where(predicate);

            query = ApplyIncludes(query, includes);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IQueryable<T>> GetQueryableAsync(
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet.AsQueryable();
            query = ApplyIncludes(query, includes);
            return query;
        }

        public async Task<bool> HasAnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var result = await dbSet.AnyAsync(predicate);
            return result;
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = dbSet.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            return await query.CountAsync(cancellationToken);
        }

        public async Task<decimal> SumAsync(
            Expression<Func<T, decimal>> selector,
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = dbSet.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            return await query.SumAsync(selector, cancellationToken);
        }

        public async Task<decimal> AvgAsync(
            Expression<Func<T, decimal>> selector,
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = dbSet.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            return await query.AverageAsync(selector, cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await dbContext.Set<T>().AddAsync(entity, cancellationToken);
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public void Delete(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public async Task<bool> DeleteByIdAsync(object id)
        {
            var entity = await dbSet.FindAsync(new object[] { id });
            if (entity == null) return false;
            dbSet.Remove(entity);
            return true;
        }

        private IQueryable<T> ApplyIncludes(IQueryable<T> query, params Expression<Func<T, object>>[] includes)
        {
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }
    }
}