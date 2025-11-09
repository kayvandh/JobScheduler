using Framework.Persistance.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Framework.Persistance
{
    public class UnitOfWork(DbContext dbContext) : IUnitOfWork
    {
        protected readonly DbContext dbContext = dbContext;
        private IDbContextTransaction? _transaction;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                    await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            dbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}