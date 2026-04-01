using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Inventory.Domain.Entities.Products;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class UoMConfiguration : BaseEntityConfiguration<UoM>
{
    public override void Configure(EntityTypeBuilder<UoM> builder)
    {
        base.Configure(builder);

        // Table name
        builder.ToTable("UoMs");

        // Name - Required, MaxLength 100
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

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


