using Inventory.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class InventoryCostLayerConfiguration : BaseEntityConfiguration<InventoryCostLayer>
{
    public override void Configure(EntityTypeBuilder<InventoryCostLayer> builder)
    {
        base.Configure(builder);

        builder.ToTable("InventoryCostLayers");

        builder.Property(x => x.OriginalQty)
            .HasPrecision(18, 4);

        builder.Property(x => x.RemainingQty)
            .HasPrecision(18, 4);

        builder.Property(x => x.ReservedQty)
            .HasPrecision(18, 4);

        builder.Property(x => x.UnitCost)
            .HasPrecision(18, 4);
    }
}


