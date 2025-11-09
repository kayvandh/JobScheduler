using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobScheduler.Domain.Entities;

namespace JobScheduler.Infrastructure.Persistence.Configurations
{
    public class StepConfiguration : IEntityTypeConfiguration<Step>
    {
        public void Configure(EntityTypeBuilder<Step> builder)
        {
            builder.ToTable("Steps");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.JobId)
                .IsRequired();

            builder.Property(s => s.Order)
                .IsRequired();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.ApiEndpoint)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(s => s.HttpMethod)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.AuthenticationType)
                .IsRequired();

            builder.Property(e => e.CreatedBy).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedBy).IsRequired(false);
            builder.Property(e => e.UpdatedAt).IsRequired(false);

            builder.OwnsOne(s => s.AuthenticationInfo, ai =>
            {
                ai.Property(a => a.Token).HasMaxLength(1000);
                ai.Property(a => a.ApiKey).HasMaxLength(1000);
                ai.Property(a => a.Username).HasMaxLength(200);
                ai.Property(a => a.Password).HasMaxLength(200);
            });

            builder.OwnsOne(s => s.RetryPolicy, rp =>
            {
                rp.Property(r => r.MaxRetries).IsRequired();
                rp.Property(r => r.DelayBetweenRetriesSeconds).IsRequired();
            });

            builder.Property(s => s.TimeoutInSeconds).IsRequired();

            builder.Property(s => s.Output)
                .HasColumnType("nvarchar(max)");

            builder.OwnsMany(s => s.Headers, h =>
            {
                h.ToTable("StepHeaders");
                h.WithOwner().HasForeignKey("StepId");
                h.HasKey("Id");
                h.Property<Guid>("Id").ValueGeneratedOnAdd();
                h.Property(p => p.Key).HasMaxLength(200).IsRequired();
                h.Property(p => p.Value).HasMaxLength(2000);
                h.Property(p => p.IsDynamic).IsRequired();
                h.Property(p => p.Source).HasMaxLength(1000);
            });

            builder.OwnsMany(s => s.QueryParameters, q =>
            {
                q.ToTable("StepQueryParameters");
                q.WithOwner().HasForeignKey("StepId");
                q.HasKey("Id");
                q.Property<Guid>("Id").ValueGeneratedOnAdd();
                q.Property(p => p.Key).HasMaxLength(200).IsRequired();
                q.Property(p => p.Value).HasMaxLength(2000);
                q.Property(p => p.IsDynamic).IsRequired();
                q.Property(p => p.Source).HasMaxLength(1000);
            });

            builder.OwnsMany(s => s.BodyParameters, b =>
            {
                b.ToTable("StepBodyParameters");
                b.WithOwner().HasForeignKey("StepId");
                b.HasKey("Id");
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property(p => p.Key).HasMaxLength(200).IsRequired();
                b.Property(p => p.Value).HasMaxLength(4000);
                b.Property(p => p.IsDynamic).IsRequired();
                b.Property(p => p.Source).HasMaxLength(1000);
            });

            builder.HasMany(s => s.ConditionalFlows)
                .WithOne()
                .HasForeignKey(cf => cf.StepId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(s => s.Status).IsRequired();

            builder.HasIndex(s => s.JobId);
            builder.HasIndex(s => new { s.JobId, s.Order }).IsUnique();
        }
    }
}