using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobScheduler.Domain.Entities;

namespace JobScheduler.Infrastructure.Persistence.Configurations
{
    public class ExecutionHistoryConfiguration : IEntityTypeConfiguration<ExecutionHistory>
    {
        public void Configure(EntityTypeBuilder<ExecutionHistory> builder)
        {
            builder.ToTable("ExecutionHistories");

            builder.HasKey(eh => eh.Id);

            builder.Property(eh => eh.StartTime)
                .IsRequired();

            builder.Property(eh => eh.EndTime)
                .IsRequired(false);

            builder.Property(eh => eh.Status)
                .IsRequired();

            builder.Property(eh => eh.RetryNumber)
                .IsRequired();

            builder.Property(eh => eh.Output)
                .HasColumnType("nvarchar(max)");

            builder.Property(eh => eh.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(s => s.TriggerSource)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(eh => eh.StepId);

            builder.Ignore(p => p.TriggeredByUser);
        }
    }
}