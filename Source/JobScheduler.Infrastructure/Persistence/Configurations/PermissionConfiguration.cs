using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobScheduler.Domain.Entities;

namespace JobScheduler.Infrastructure.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(p => p.ApiPath)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(p => p.HttpMethod)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(p => p.Description)
                   .HasMaxLength(500);

            builder.HasMany(p => p.RolePermissions)
                   .WithOne(rp => rp.Permission)
                   .HasForeignKey(rp => rp.PermissionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}