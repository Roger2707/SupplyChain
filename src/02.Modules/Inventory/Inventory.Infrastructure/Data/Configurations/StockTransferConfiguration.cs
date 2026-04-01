using Inventory.Domain.Entities.StockTransfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class StockTransferConfiguration : BaseEntityConfiguration<StockTransfer>
{
    public override void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        base.Configure(builder);

        builder.ToTable("StockTransfers");

        builder.Property(x => x.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);
    }
}


