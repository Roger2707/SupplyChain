using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SharedKernel.Entities;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> 
    where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Timestamp for optimistic concurrency
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Index on IsDeleted for better query performance
        builder.HasIndex(e => e.IsDeleted);
    }
}


