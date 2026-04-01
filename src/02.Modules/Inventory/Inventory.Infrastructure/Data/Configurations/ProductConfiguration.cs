using Inventory.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        // Table name
        builder.ToTable("Products");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.SKU)
            .IsUnique();

        builder.Property(p => p.Barcode)
            .HasMaxLength(13)
            .IsRequired();

        builder.HasIndex(p => p.Barcode)
            .IsUnique();

        builder.Property(x => x.MinStockLevel)
            .HasPrecision(18, 4);

        builder.Property(x => x.SellingPrice)
            .HasPrecision(18, 4);
        
        // Category
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Base UoM
        builder.HasOne(x => x.BaseUoM)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.BaseUoMId)
            .OnDelete(DeleteBehavior.Restrict);

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


