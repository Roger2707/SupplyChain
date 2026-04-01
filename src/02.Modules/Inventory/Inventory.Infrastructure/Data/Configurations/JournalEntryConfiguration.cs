using Inventory.Domain.Entities.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Inventory.Infrastructure.Data.Configurations;

public class JournalEntryConfiguration : BaseEntityConfiguration<JournalEntry>
{
    public override void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("JournalEntries");

        builder.Property(x => x.PostingDate)
            .IsRequired();

        builder.Property(x => x.Reference)
            .IsRequired();
    }
}


