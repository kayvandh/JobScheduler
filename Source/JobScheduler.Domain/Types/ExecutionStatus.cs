namespace JobScheduler.Domain.Types
{
    public enum ExecutionStatus
    {
        Pending = 1,
        Running = 2,
        Completed = 3,
        Failed = 4
    }
}