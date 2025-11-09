namespace JobScheduler.Application.Common.Interfaces.Identity
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }

        bool IsInRole(string role);
    }
}