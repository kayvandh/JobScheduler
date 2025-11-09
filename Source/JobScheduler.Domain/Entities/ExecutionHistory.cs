using JobScheduler.Domain.Types;

namespace JobScheduler.Domain.Entities
{
    public class ExecutionHistory
    {
        public Guid Id { get; private set; }
        public Guid StepId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public ExecutionStatus Status { get; set; }
        public int RetryNumber { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? TriggeredByUserId { get; set; }
        public TriggerSource TriggerSource { get; set; }

        public ExecutionHistory()
        {
            Id = Guid.NewGuid();
        }

        public void MarkStarted(Guid? userId, TriggerSource source)
        {
            StartTime = DateTime.UtcNow;
            TriggeredByUserId = userId;
            TriggerSource = source;
            Status = ExecutionStatus.Running;
        }

        public void MarkCompleted()
        {
            EndTime = DateTime.UtcNow;
            Status = ExecutionStatus.Completed;
        }

        public void MarkFailed(string? errorMessage = null)
        {
            EndTime = DateTime.UtcNow;
            Status = ExecutionStatus.Failed;
            ErrorMessage = errorMessage;
        }
    }
}