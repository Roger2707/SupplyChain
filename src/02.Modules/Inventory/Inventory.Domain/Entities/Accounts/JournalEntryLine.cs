using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Accounts
{
    public class JournalEntryLine : BaseEntity
    {
        public int JournalEntryId { get; set; }

        public JournalEntry JournalEntry { get; set; }

        public int AccountId { get; set; }

        public Account Account { get; set; }

        public decimal Debit { get; set; } = 0;

        public decimal Credit { get; set; } = 0;

        public string Description { get; set; } = string.Empty;
    }
}
