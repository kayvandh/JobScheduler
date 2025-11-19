namespace JobScheduler.Domain.Interfaces
{
    public interface IBaseUser
    {
        Guid Id { get; }
        string DisplayName { get; }
    }
}
