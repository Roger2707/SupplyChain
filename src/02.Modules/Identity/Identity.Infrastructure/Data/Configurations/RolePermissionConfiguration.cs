using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        // Table name
        builder.ToTable("RolePermissions");

        // Composite Primary Key
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // RoleId - Required Foreign Key
        builder.Property(rp => rp.RoleId)
            .IsRequired();

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // PermissionId - Required Foreign Key
        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // GrantedAt - Required, Default DateTime.Now
        builder.Property(rp => rp.GrantedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Composite index for better query performance
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId });
    }
}

