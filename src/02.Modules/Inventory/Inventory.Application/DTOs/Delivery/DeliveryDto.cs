using Inventory.Domain.Enums;

namespace Inventory.Application.DTOs.Delivery
{
    public class DeliveryDto
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DeliveryStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        public byte[]? RowVersion { get; set; }

        public List<DeliveryLineDto> LinesDto { get; set; } = new();
    }

    public class DeliveryLineDto
    {
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal InvoicedQty { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }
}
