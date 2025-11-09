using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobScheduler.Domain.Entities;
using JobScheduler.Infrastructure.Identity.Entities;
using JobScheduler.Infrastructure.Persistence.Interfaces;

namespace JobScheduler.Infrastructure.Persistence
{
    public class JobSchedulerDbContext(DbContextOptions<JobSchedulerDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), ISeeder
    {
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Step> Steps => Set<Step>();
        public DbSet<ConditionalFlow> ConditionalFlows => Set<ConditionalFlow>();
        public DbSet<ExecutionHistory> ExecutionHistories => Set<ExecutionHistory>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobSchedulerDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public void Seed()
        {
            return;
        }
    }
}