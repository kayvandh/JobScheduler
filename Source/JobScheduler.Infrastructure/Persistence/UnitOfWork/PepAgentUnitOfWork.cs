using Framework.Persistance.Interfaces;
using Microsoft.EntityFrameworkCore;
using JobScheduler.Application.Common.Interfaces.Identity;
using JobScheduler.Application.Common.Interfaces.Persistence;
using JobScheduler.Domain.Common;
using JobScheduler.Domain.Entities;
using JobScheduler.Infrastructure.Persistence.Repositories;

namespace JobScheduler.Infrastructure.Persistence.UnitOfWork
{
    public class JobSchedulerUnitOfWork : Framework.Persistance.UnitOfWork, IJobSchedulerUnitOfWork
    {
        private readonly ICurrentUserService currentUserService;

        public IGenericRepository<Job> Jops { get; }
        public IGenericRepository<Step> Steps { get; }
        public IGenericRepository<ConditionalFlow> ConditionalFlows { get; }
        public IGenericRepository<ExecutionHistory> ExecutionHistories { get; }
        public IGenericRepository<Permission> Permissions { get; }
        public IGenericRepository<RolePermission> RolePermissions { get; }
        public IGenericRepository<RefreshToken> RefreshTokens { get; }

        public JobSchedulerUnitOfWork(
            JobSchedulerDbContext dbContext,
            ICurrentUserService currentUserService,
            IGenericRepository<Job> jops,
            IGenericRepository<Step> steps,
            IGenericRepository<ConditionalFlow> conditionalFlows,
            IGenericRepository<ExecutionHistory> executionHistories,
            CachedRepository<Permission> permissions,
            CachedRepository<RolePermission> rolePermissions,
            IGenericRepository<RefreshToken> refreshTokens) : base(dbContext)
        {
            this.currentUserService = currentUserService;
            Jops = jops;
            Steps = steps;
            ConditionalFlows = conditionalFlows;
            ExecutionHistories = executionHistories;
            Permissions = permissions;
            RolePermissions = rolePermissions;
            RefreshTokens = refreshTokens;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var userId = currentUserService.UserId ?? Guid.Empty;

            var entries = dbContext.ChangeTracker
                .Entries()
                .Where(e => e.Entity is AuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (AuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.SetCreated(userId);
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.SetUpdated(userId);
                }
            }
        }
    }
}