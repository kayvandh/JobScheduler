using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobScheduler.Infrastructure.Identity.Entities;

namespace JobScheduler.Infrastructure.Identity.Services
{
    public class AuthenticationService : Application.Common.Interfaces.Identity.IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Guid?> ValidateUserAsync(string username, string password)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null || !user.IsActive)
                return null;

            var valid = await _userManager.CheckPasswordAsync(user, password);
            return valid ? user.Id : null;
        }

        public async Task<bool> IsUserValidOrActiveAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null && user.IsActive;
        }
    }
}