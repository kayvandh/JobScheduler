using Framework.Cache.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobScheduler.Application.Common.Interfaces.Identity;
using JobScheduler.Infrastructure.Identity.Entities;
using JobScheduler.Infrastructure.Persistence;
using System.Security.Claims;

namespace JobScheduler.Infrastructure.Identity.Services
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JobSchedulerDbContext _context;
        private readonly ICacheService _cacheService;

        private static readonly string CacheKeyPrefix = "permissions:roles:";
        private readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(60);

        private record PermissionEntry(string Path, string Method);

        public AuthorizationService(
            UserManager<ApplicationUser> userManager,
            JobSchedulerDbContext context,
            ICacheService cacheService)
        {
            _userManager = userManager;
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<bool> AuthorizeAsync(ClaimsPrincipal user, string path, string httpMethod)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return false;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return false;

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
                return false;

            var roles = await _userManager.GetRolesAsync(appUser);
            if (!roles.Any())
                return false;

            foreach (var roleName in roles)
            {
                var cacheKey = $"{CacheKeyPrefix}{roleName.ToLower()}";

                var permissions = await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async _ =>
                    {
                        var result = await (
                            from rp in _context.RolePermissions
                            join p in _context.Permissions on rp.PermissionId equals p.Id
                            join r in _context.Roles on rp.RoleId equals r.Id
                            where r.Name == roleName
                            select new PermissionEntry(p.ApiPath, p.HttpMethod)
                        ).ToListAsync();

                        return result;
                    });

                if (permissions.Any(p =>
                    p.Path.Equals(path, StringComparison.OrdinalIgnoreCase) &&
                    p.Method.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}