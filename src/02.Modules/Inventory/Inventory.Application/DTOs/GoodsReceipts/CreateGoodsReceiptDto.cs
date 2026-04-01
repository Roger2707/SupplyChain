namespace Inventory.Application.DTOs.GoodsReceipts
{
    public class CreateGoodsReceiptDto
    {
        public int PurchaseOrderId { get; set; }
        public int WarehouseId { get; set; }
        public List<CreateGoodsReceiptLineDto> LinesDto { get; set; }
    }

    public class CreateGoodsReceiptLineDto
    {
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public decimal ReceivedQty { get; set; }
    }
}
