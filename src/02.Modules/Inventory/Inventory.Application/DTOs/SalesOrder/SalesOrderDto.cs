using Inventory.Domain.Enums;

namespace Inventory.Application.DTOs.SalesOrder
{
    public class SalesOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public SalesOrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        // Concurrency token so client can send back when updating
        public byte[]? RowVersion { get; set; }

        public List<SalesOrderLineDto> LinesDto { get; set; } = new();
    }

    public class SalesOrderLineDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RemainingQty { get; set; }
        public decimal LineTotal { get; set; }
    }
}
