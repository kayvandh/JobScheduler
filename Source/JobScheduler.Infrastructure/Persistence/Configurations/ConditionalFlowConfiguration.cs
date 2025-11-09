using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobScheduler.Domain.Entities;

namespace JobScheduler.Infrastructure.Persistence.Configurations
{
    public class ConditionalFlowConfiguration : IEntityTypeConfiguration<ConditionalFlow>
    {
        public void Configure(EntityTypeBuilder<ConditionalFlow> builder)
        {
            builder.ToTable("ConditionalFlows");

            builder.HasKey(cf => cf.Id);

            builder.Property(cf => cf.StepId).IsRequired();

            builder.Property(cf => cf.StatusCode).IsRequired();

            builder.Property(cf => cf.Description).HasMaxLength(500);

            // Relationship: ConditionalFlow -> Step (owner)
            builder.HasOne<Step>()
                .WithMany(s => s.ConditionalFlows)
                .HasForeignKey(cf => cf.StepId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: ConditionalFlow -> NextStep (optional reference)
            builder.HasOne<Step>()
                .WithMany()
                .HasForeignKey(cf => cf.NextStepId)
                .OnDelete(DeleteBehavior.NoAction); // deleting the referenced NextStep should not cascade-delete the flow

            // Indexes
            builder.HasIndex(cf => cf.StepId);
            builder.HasIndex(cf => new { cf.StepId, cf.StatusCode });
        }
    }
}