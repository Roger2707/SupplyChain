namespace Inventory.Application.DTOs.GoodsReceipts
{
    public class UpdateGoodsReceiptDto
    {
        public int WarehouseId { get; set; }
        public byte[]? RowVersion { get; set; }
        public List<UpdateGoodsReceiptLineDto> LinesDto { get; set; }
    }

    public class UpdateGoodsReceiptLineDto
    {
        public int ProductId { get; set; }
        public decimal ReceivedQty { get; set; }
    }
}
