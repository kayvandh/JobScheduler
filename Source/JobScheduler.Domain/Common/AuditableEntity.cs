namespace JobScheduler.Domain.Common
{
    /// <summary>
    /// Base entity for tracking creation and modification audit information.
    /// </summary>
    public abstract class AuditableEntity
    {
        /// <summary>
        /// The user who created this record.
        /// </summary>
        public Guid CreatedBy { get; private set; }

        /// <summary>
        /// The timestamp when this record was created (in UTC).
        /// </summary>
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// The user who last updated this record.
        /// </summary>
        public Guid? UpdatedBy { get; private set; }

        /// <summary>
        /// The timestamp when this record was last updated (in UTC).
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Sets audit fields for entity creation.
        /// </summary>
        public void SetCreated(Guid userId)
        {
            CreatedBy = userId;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets audit fields for entity update.
        /// </summary>
        public void SetUpdated(Guid userId)
        {
            UpdatedBy = userId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}