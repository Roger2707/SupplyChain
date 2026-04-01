using ECommerce.Domain.Entities.Baskets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class BasketConfiguration : BaseEntityConfiguration<Basket>
    {
        public override void Configure(EntityTypeBuilder<Basket> builder)
        {
            base.Configure(builder);
            builder.ToTable("Baskets");
        }
    }
}
