using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;
using SharedKernel.Entities;

namespace Identity.Infrastructure.Data.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        // Table name
        builder.ToTable("Users");

        // Username - Required, MaxLength 50, Unique
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // PasswordHash - Required, MaxLength 255
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        // FullName - Required, MaxLength 100
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);

        // Email - Required, MaxLength 100, Unique
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // PhoneNumber - Optional, MaxLength 20
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        // Address - Optional, MaxLength 255
        builder.Property(u => u.Address)
            .HasMaxLength(255);

        // IsActive - Default true
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation properties
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


