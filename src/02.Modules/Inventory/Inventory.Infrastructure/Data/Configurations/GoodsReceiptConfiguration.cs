using Inventory.Domain.Entities.GoodsReceipt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class GoodsReceiptConfiguration : BaseEntityConfiguration<GoodsReceipt>
{
    public override void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        base.Configure(builder);

        builder.ToTable("GoodsReceipts");

        builder.Property(x => x.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ReceiptDate)
            .IsRequired();
    }
}


