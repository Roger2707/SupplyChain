using Inventory.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class InventoryLedgerConfiguration : BaseEntityConfiguration<InventoryLedger>
{
    public override void Configure(EntityTypeBuilder<InventoryLedger> builder)
    {
        base.Configure(builder);

        builder.ToTable("InventoryLedgers");

        builder.Property(x => x.QuantityIn)
            .HasPrecision(18, 4);

        builder.Property(x => x.QuantityOut)
            .HasPrecision(18, 4);

        builder.Property(x => x.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(x => x.TotalCost)
            .HasPrecision(18, 4);
    }
}


