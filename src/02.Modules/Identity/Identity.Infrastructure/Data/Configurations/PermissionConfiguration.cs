using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;
using SharedKernel.Entities;

namespace Identity.Infrastructure.Data.Configurations;

public class PermissionConfiguration : BaseEntityConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        base.Configure(builder);

        // Table name
        builder.ToTable("Permissions");

        // PermissionName - Required, MaxLength 100, Unique
        builder.Property(p => p.PermissionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.PermissionName)
            .IsUnique();

        // Module - Required, MaxLength 50
        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(50);

        // Action - Required, MaxLength 50
        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(50);

        // Description - Optional, MaxLength 255
        builder.Property(p => p.Description)
            .HasMaxLength(255);

        // Composite index on Module and Action for better query performance
        builder.HasIndex(p => new { p.Module, p.Action });

        // Navigation properties
        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


