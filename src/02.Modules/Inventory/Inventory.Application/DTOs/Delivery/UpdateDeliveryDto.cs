namespace Inventory.Application.DTOs.Delivery
{
    public class UpdateDeliveryDto
    {
        public byte[]? RowVersion { get; set; }

        public List<UpdateDeliveryLineDto> LinesDto { get; set; } = new();
    }

    public class UpdateDeliveryLineDto
    {
        public int ProductId { get; set; }
        public int RowNumber { get; set; }
        public decimal DeliveredQty { get; set; }
    }
}
