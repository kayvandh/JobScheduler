namespace Framework.Persistance.Interfaces
{
    public interface ISecureUnitOfWork : IUnitOfWork
    {
        Task<bool> CanCreateAsync<T>(CancellationToken cancellationToken = default) where T : class, ISecureEntity;

        Task<bool> CanReadAsync<T>(CancellationToken cancellationToken = default) where T : class, ISecureEntity;

        Task<bool> CanUpdateAsync<T>(CancellationToken cancellationToken = default) where T : class, ISecureEntity;

        Task<bool> CanDeleteAsync<T>(CancellationToken cancellationToken = default) where T : class, ISecureEntity;
    }
}