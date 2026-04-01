using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Accounts
{
    public class JournalEntry : BaseEntity
    {
        public DateTime PostingDate { get; set; } = DateTime.UtcNow;
        public string Reference { get; set; }
        public int? GoodsReceiptId { get; set; }
        public int? DeliveryId { get; set; }
        public int? InvoiceId { get; set; }
        public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }
}
