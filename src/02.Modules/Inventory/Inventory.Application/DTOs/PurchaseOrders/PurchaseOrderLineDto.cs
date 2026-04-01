namespace Inventory.Application.DTOs.PurchaseOrders;

public class PurchaseOrderLineDto
{
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

