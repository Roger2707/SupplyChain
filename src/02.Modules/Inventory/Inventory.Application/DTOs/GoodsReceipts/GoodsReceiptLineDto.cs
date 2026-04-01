namespace Inventory.Application.DTOs.GoodsReceipts;

public class GoodsReceiptLineDto
{
    public int GoodsReceiptId { get; set; }
    public int PurchaseOrderLId { get; set; }
    public int ProductId { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}

