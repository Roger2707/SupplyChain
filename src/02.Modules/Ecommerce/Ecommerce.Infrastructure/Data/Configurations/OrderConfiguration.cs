using ECommerce.Domain.Entities.Baskets;
using ECommerce.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : BaseEntityConfiguration<Order>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);
                builder.ToTable("Orders");

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 4);
        }
    }
}
