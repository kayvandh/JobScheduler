using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobScheduler.Domain.Entities;

namespace JobScheduler.Infrastructure.Persistence.Configurations
{
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.ToTable("Jobs");

            builder.HasKey(j => j.Id);

            builder.Property(j => j.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.Description)
                .HasMaxLength(1000);

            builder.Property(j => j.CronSchedule)
                .HasMaxLength(200);

            builder.Property(j => j.IsOneTime)
                .IsRequired();

            builder.Property(j => j.Status)
                .IsRequired();

            builder.Property(e => e.CreatedBy).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedBy).IsRequired(false);
            builder.Property(e => e.UpdatedAt).IsRequired(false);

            builder.HasMany(j => j.Steps)
                .WithOne()
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}