using ECommerce.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
                builder.Property(x => x.Quantity).HasPrecision(18, 4);
                builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
                builder.Ignore(x => x.LineTotal);
        }
    }
}
