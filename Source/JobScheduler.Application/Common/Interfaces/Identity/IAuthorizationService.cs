using System.Security.Claims;

namespace JobScheduler.Application.Common.Interfaces.Identity
{
    public interface IAuthorizationService
    {
        Task<bool> AuthorizeAsync(ClaimsPrincipal user, string path, string httpMethod);
    }
}