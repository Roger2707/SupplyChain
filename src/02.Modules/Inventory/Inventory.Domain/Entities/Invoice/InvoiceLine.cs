using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Invoice
{
    public class InvoiceLine : BaseEntity
    {
        public int InvoiceId { get; set; }
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public int RowNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}