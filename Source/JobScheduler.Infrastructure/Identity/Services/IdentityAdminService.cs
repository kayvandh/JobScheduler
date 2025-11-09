using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobScheduler.Application.Common.Interfaces.Identity;
using JobScheduler.Application.Common.Interfaces.Persistence;
using JobScheduler.Domain.Entities;
using JobScheduler.Infrastructure.Identity.Entities;

namespace JobScheduler.Infrastructure.Identity.Services
{
    public class IdentityAdminService : IIdentityAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IJobSchedulerUnitOfWork _unitOfWork;

        public IdentityAdminService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IJobSchedulerUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateUserAsync(string username, string password, string roleName)
        {
            var user = new ApplicationUser { UserName = username, Email = $"{username}@domain.com" };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrEmpty(roleName))
                await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CreateRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }

        public async Task<IEnumerable<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null) return await _userManager.GetRolesAsync(user);
            else return Enumerable.Empty<string>();
        }

        public async Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId)
        {
            var exists = await _unitOfWork.RolePermissions.HasAnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (!exists)
            {
                await _unitOfWork.RolePermissions.AddAsync(new RolePermission()
                {
                    PermissionId = permissionId,
                    RoleId = roleId
                });

                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
        {
            var entity = await _unitOfWork.RolePermissions
                .GetOneAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (entity != null)
            {
                _unitOfWork.RolePermissions.Delete(entity);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<Permission> CreatePermissionAsync(string apiPath, string httpMethod, string? description = null)
        {
            var permission = new Permission
            {
                ApiPath = apiPath,
                HttpMethod = httpMethod,
                Description = description
            };

            await _unitOfWork.Permissions.AddAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return permission;
        }

        public async Task<bool> DeletePermissionAsync(Guid permissionId)
        {
            return await _unitOfWork.Permissions.DeleteByIdAsync(permissionId);
        }

        public async Task AssignPermissionToRoleAsync(Guid permissionId, Guid roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new InvalidOperationException("Role not found.");

            var exists = await _unitOfWork.RolePermissions.HasAnyAsync(rp => rp.PermissionId == permissionId && rp.RoleId == roleId);
            if (exists) return;

            var rolePermission = new RolePermission
            {
                PermissionId = permissionId,
                RoleId = roleId
            };

            await _unitOfWork.RolePermissions.AddAsync(rolePermission);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AssignRoleToUserAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new InvalidOperationException("User not found.");

            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new InvalidOperationException($"Role '{roleName}' does not exist.");

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new InvalidOperationException("User not found.");

            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new InvalidOperationException($"Role '{roleName}' does not exist.");

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return (await _unitOfWork.Permissions.GetAllAsync()).Items;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            var result = await _unitOfWork.RolePermissions.GetAsync(p => p.RoleId == roleId, includes: rp => rp.Permission!);
            return result.Items.Select(rp => rp.Permission!);
        }
    }
}