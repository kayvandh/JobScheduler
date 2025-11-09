namespace JobScheduler.Domain.Entities
{
    public class ConditionalFlow
    {
        public Guid Id { get; private set; }
        public Guid StepId { get; set; }
        public Guid? NextStepId { get; set; }
        public int StatusCode { get; set; }
        public string? Description { get; set; }

        public ConditionalFlow()
        {
            Id = Guid.NewGuid();
        }
    }
}