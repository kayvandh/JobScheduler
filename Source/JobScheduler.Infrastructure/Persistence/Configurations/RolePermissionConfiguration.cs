using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobScheduler.Domain.Entities;

namespace JobScheduler.Infrastructure.Persistence.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");

            builder.HasKey(rp => rp.Id);

            builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                   .IsUnique();

            builder.Property(rp => rp.RoleId)
                   .IsRequired();

            builder.Property(rp => rp.PermissionId)
                   .IsRequired();

            builder.HasOne<IdentityRole<Guid>>() // بدون navigation
                   .WithMany()
                   .HasForeignKey(rp => rp.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}