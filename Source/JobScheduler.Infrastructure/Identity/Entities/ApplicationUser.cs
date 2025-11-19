using Microsoft.AspNetCore.Identity;
using JobScheduler.Domain.Entities;
using JobScheduler.Domain.Interfaces;

namespace JobScheduler.Infrastructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, IBaseUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? SecretKey { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}