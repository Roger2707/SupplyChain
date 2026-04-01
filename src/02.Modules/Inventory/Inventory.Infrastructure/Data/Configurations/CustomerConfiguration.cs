using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Inventory.Domain.Entities;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class CustomerConfiguration : BaseEntityConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        // Table name
        builder.ToTable("Customers");

        // CustomerCode - Required, MaxLength 20, Unique
        builder.Property(w => w.CustomerCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(w => w.CustomerCode)
            .IsUnique();

        // CustomerName - Required, MaxLength 100
        builder.Property(w => w.CustomerName)
            .IsRequired()
            .HasMaxLength(100);

        // Address - Optional, MaxLength 255
        builder.Property(w => w.Address)
            .HasMaxLength(255);

        // PhoneNumber - Optional, MaxLength 20
        builder.Property(w => w.PhoneNumber)
            .HasMaxLength(20);

        // IsActive - Default true
        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Description - Optional, MaxLength 500
        builder.Property(w => w.Description)
            .HasMaxLength(500);

        // CreatedAt - Required
        builder.Property(w => w.CreatedAt)
            .IsRequired();

        // UpdatedAt - Optional
        builder.Property(w => w.UpdatedAt)
            .IsRequired(false);

        // IsDeleted - Required, Default false
        builder.Property(w => w.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}


