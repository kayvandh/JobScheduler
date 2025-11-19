using JobScheduler.Domain.Entities;
using JobScheduler.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.Property(e => e.CreatedByUserId).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedByUserId).IsRequired(false);
            builder.Property(e => e.UpdatedAt).IsRequired(false);

            builder.HasMany(j => j.Steps)
                .WithOne()
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(p => p.CreatedByUser);
            builder.Ignore(p => p.UpdatedByUser);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(j => j.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(j => j.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}