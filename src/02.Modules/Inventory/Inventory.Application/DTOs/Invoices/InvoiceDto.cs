using Inventory.Domain.Enums;

namespace Inventory.Application.DTOs.Invoices
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public int DeliveryId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public List<InvoiceLineDto> Lines { get; set; }
    }

    public class InvoiceLineDto
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }

        // FK: Delivery
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public int RowNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
