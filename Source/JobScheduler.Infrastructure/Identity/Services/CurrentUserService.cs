using Microsoft.AspNetCore.Http;
using JobScheduler.Application.Common.Interfaces.Identity;
using System.Security.Claims;

namespace JobScheduler.Infrastructure.Identity.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public Guid? UserId =>
            Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id
                : null;

        public string? UserName =>
            _httpContextAccessor.HttpContext?.User.Identity?.Name;

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role) =>
            _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}