using JobScheduler.Domain.Interfaces;

namespace JobScheduler.Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid CreatedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Guid? UpdatedByUserId { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public IBaseUser? CreatedByUser { get; set; }
        public IBaseUser? UpdatedByUser { get; set; }

        public void SetCreated(Guid userId)
        {
            CreatedByUserId = userId;
            CreatedAt = DateTime.UtcNow;
        }
        public void SetUpdated(Guid userId)
        {
            UpdatedByUserId = userId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}