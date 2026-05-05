using Inventory.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class InventoryReservationConfiguration : BaseEntityConfiguration<InventoryReservation>
{
    public override void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        base.Configure(builder);

        builder.ToTable("InventoryReservations");

        builder.Property(x => x.ReservedQty)
            .HasPrecision(18, 4);

        builder.Property(x => x.UnitCost)
            .HasPrecision(18, 4);

        builder.Ignore(x => x.TotalAmount);

        // Unique Index: ensure that no row in table has the same data
        builder
            .HasIndex(r => new { r.SourceId, r.SourceType, r.RowNumber }) // SourceType: SalesOrder || Order
            .IsUnique();
    }
}


