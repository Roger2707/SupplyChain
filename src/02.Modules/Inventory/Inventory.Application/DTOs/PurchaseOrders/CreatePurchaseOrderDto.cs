namespace Inventory.Application.DTOs.PurchaseOrders
{
    public class CreatePurchaseOrderDto
    {
        public int SupplierId { get; set; }
        public List<CreatePurchaseOrderLineDto> LinesDto { get; set; } = new();
    }

    public class CreatePurchaseOrderLineDto
    {
        public int ProductId { get; set; }
        public decimal OrderedQty { get; set; }
    }
}
