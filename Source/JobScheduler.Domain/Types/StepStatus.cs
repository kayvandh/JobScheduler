namespace JobScheduler.Domain.Types
{
    public enum StepStatus
    {
        Pending = 1,
        Running = 2,
        Completed = 3,
        Failed = 4
    }
}