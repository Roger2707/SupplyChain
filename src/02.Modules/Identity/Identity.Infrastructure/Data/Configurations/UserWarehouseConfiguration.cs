using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Data.Configurations;

public class UserWarehouseConfiguwation : IEntityTypeConfiguration<UserWarehouse>
{
    public void Configure(EntityTypeBuilder<UserWarehouse> builder)
    {
        // Table name
        builder.ToTable("UserWarehouses");

        // Composite Primary Key
        builder.HasKey(uw => new { uw.UserId, uw.WarehouseId });

        // UserId - Required Foreign Key
        builder.Property(uw => uw.UserId)
            .IsRequired();

        // RoleId - Required Foreign Key
        builder.Property(uw => uw.WarehouseId)
            .IsRequired();

        // Composite index for better query performance
        builder.HasIndex(uw => new { uw.UserId, uw.WarehouseId });
    }
}

