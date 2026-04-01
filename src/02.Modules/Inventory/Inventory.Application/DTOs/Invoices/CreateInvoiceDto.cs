namespace Inventory.Application.DTOs.Invoices
{
    public class CreateInvoiceDto
    {
        public int DeliveryId { get; set; }
        public List<CreateInvoiceLineDto> CreateInvoiceLineDtos { get; set; } = new();
    }

    public class CreateInvoiceLineDto
    {
        public int ProductId { get; set; }
        public int RowNumber { get; set; }
        public int InvoiceQuantity { get; set; }
    }
}
