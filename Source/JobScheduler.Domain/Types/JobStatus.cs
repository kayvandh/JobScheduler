namespace JobScheduler.Domain.Types
{
    public enum JobStatus
    {
        Pending = 1,
        Running = 2,
        Completed = 3,
        Failed = 4
    }
}