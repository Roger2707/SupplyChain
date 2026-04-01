using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Inventory.Domain.Entities.Products;

namespace Inventory.Infrastructure.Data.Configurations.Identity;

public class ProductUoMConversionConfiguration : IEntityTypeConfiguration<ProductUoMConversion>
{
    public void Configure(EntityTypeBuilder<ProductUoMConversion> builder)
    {
        // Table name
        builder.ToTable("ProductUoMConversions");

        // Composite Primary Key
        builder.HasKey(pc => new { pc.ProductId, pc.FromUoMId, pc.ToUoMId });

        builder.Property(x => x.Factor).HasPrecision(18, 6);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Conversions)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FromUoM)
            .WithMany()
            .HasForeignKey(x => x.FromUoMId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToUoM)
            .WithMany()
            .HasForeignKey(x => x.ToUoMId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite index for better query performance
        builder.HasIndex(pc => new { pc.ProductId, pc.FromUoMId, pc.ToUoMId }).IsUnique();
    }
}

