namespace Inventory.Application.DTOs.Delivery
{
    public class CreateDeliveryDto
    {
        public int SalesOrderId { get; set; }
        public List<CreateDeliveryLineDto> LinesDto { get; set; } = new();
    }

    public class CreateDeliveryLineDto
    {
        public int ProductId { get; set; }
        public int RowNumber { get; set; }
        public decimal DeliveredQty { get; set; }
    }
}
