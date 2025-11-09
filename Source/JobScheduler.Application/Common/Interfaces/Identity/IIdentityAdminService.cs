namespace JobScheduler.Application.Common.Interfaces.Identity
{
    public interface IIdentityAdminService
    {
        Task CreateUserAsync(string username, string password, string roleName);

        Task CreateRoleAsync(string roleName);

        Task<IEnumerable<string>> GetAllRolesAsync();

        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        Task AssignRoleToUserAsync(Guid userId, string roleName);

        Task RemoveRoleFromUserAsync(Guid userId, string roleName);

        Task<Domain.Entities.Permission> CreatePermissionAsync(string apiPath, string httpMethod, string? description = null);

        Task<bool> DeletePermissionAsync(Guid permissionId);

        Task<IEnumerable<Domain.Entities.Permission>> GetAllPermissionsAsync();

        Task AssignPermissionToRoleAsync(Guid permissionId, Guid roleId);

        Task RemovePermissionFromRoleAsync(Guid permissionId, Guid roleId);

        Task<IEnumerable<Domain.Entities.Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
    }
}