using JobScheduler.Domain.Common;
using JobScheduler.Domain.Types;

namespace JobScheduler.Domain.Entities
{
    public class Job : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CronSchedule { get; set; } = string.Empty;
        public bool IsOneTime { get; set; }
        public JobStatus Status { get; set; }
        public List<Step> Steps { get; set; } = [];

        public Job()
        {
            Id = Guid.NewGuid();
        }
    }
}