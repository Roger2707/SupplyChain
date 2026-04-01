using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations
{
    public class RegionConfiguration : BaseEntityConfiguration<Region>
    {
        public override void Configure(EntityTypeBuilder<Region> builder)
        {
            base.Configure(builder);

            // Table name
            builder.ToTable("Regions");

            // RegionCode - Required, MaxLength 20, Unique
            builder.Property(r => r.RegionCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(r => r.RegionCode)
                .IsUnique();

            // RegionName - Required, MaxLength 100
            builder.Property(r => r.RegionName)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}

