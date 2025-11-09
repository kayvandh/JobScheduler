namespace JobScheduler.Application.Common.Interfaces.Identity
{
    public interface IAuthenticationService
    {
        Task<Guid?> ValidateUserAsync(string username, string password);

        Task<bool> IsUserValidOrActiveAsync(Guid userId);
    }
}