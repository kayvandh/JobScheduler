using Framework.Persistance.Interfaces;
using JobScheduler.Domain.Entities;

namespace JobScheduler.Application.Common.Interfaces.Persistence
{
    public interface IJobSchedulerUnitOfWork : IUnitOfWork
    {
        public IGenericRepository<Job> Jops { get; }
        public IGenericRepository<Step> Steps { get; }
        public IGenericRepository<ConditionalFlow> ConditionalFlows { get; }
        public IGenericRepository<ExecutionHistory> ExecutionHistories { get; }
        public IGenericRepository<Permission> Permissions { get; }
        public IGenericRepository<RolePermission> RolePermissions { get; }
        public IGenericRepository<RefreshToken> RefreshTokens { get; }
    }
}