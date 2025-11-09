using Framework.Cache.Interface;
using Framework.Persistance;
using Framework.Persistance.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JobScheduler.Application.Common.Interfaces.Identity;
using JobScheduler.Application.Common.Interfaces.Persistence;
using JobScheduler.Infrastructure.Cache;
using JobScheduler.Infrastructure.Identity.Entities;
using JobScheduler.Infrastructure.Identity.Services;
using JobScheduler.Infrastructure.Persistence;
using JobScheduler.Infrastructure.Persistence.Repositories;
using JobScheduler.Infrastructure.Persistence.UnitOfWork;

namespace JobScheduler.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<JobSchedulerDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<DbContext, JobSchedulerDbContext>();

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = false;
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<JobSchedulerDbContext>()
            .AddDefaultTokenProviders();

            var cacheProvider = configuration.GetSection("CacheSettings:Provider").Value?.ToUpper();
            if (cacheProvider == "REDIS")
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("Redis");
                });
                services.AddScoped<ICacheService, RedisCacheService>();
            }
            else
            {
                services.AddMemoryCache();
                services.AddScoped<ICacheService, MemoryCacheService>();
            }

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IIdentityAdminService, IdentityAdminService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(CachedRepository<>));

            services.AddScoped<IJobSchedulerUnitOfWork, JobSchedulerUnitOfWork>();

            services.AddHttpContextAccessor();

            return services;
        }
    }
}