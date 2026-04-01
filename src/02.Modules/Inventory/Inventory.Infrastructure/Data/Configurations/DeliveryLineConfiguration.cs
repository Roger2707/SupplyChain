using Inventory.Domain.Entities.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public class DeliveryLineConfiguration : IEntityTypeConfiguration<DeliveryLine>
{
    public void Configure(EntityTypeBuilder<DeliveryLine> builder)
    {
        builder.ToTable("DeliveryLines");

        builder.HasKey(x => new { x.DeliveryId, x.ProductId, x.RowNumber });

        builder.Property(x => x.DeliveredQty)
            .HasPrecision(18, 4);

        builder.Property(x => x.InvoicedQty)
            .HasPrecision(18, 4);

        builder.Property(x => x.UnitCost)
            .HasPrecision(18, 4);

        builder.Ignore(x => x.RemainingInvoicedQty);
        builder.Ignore(x => x.LineTotal);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Delivery>()
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.DeliveryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

