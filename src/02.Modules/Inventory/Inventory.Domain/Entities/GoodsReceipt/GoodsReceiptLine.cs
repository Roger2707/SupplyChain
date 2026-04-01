namespace Inventory.Domain.Entities.GoodsReceipt
{
    public class GoodsReceiptLine
    {
        public int GoodsReceiptId { get; set; }
        public int PurchaseOrderId { get; set; }

        public int ProductId { get; set; }

        public decimal ReceivedQty { get; set; }

        // Realistic Price for FIFO
        public decimal UnitCost { get; set; }

        public decimal LineTotal => ReceivedQty * UnitCost;
    }
}
