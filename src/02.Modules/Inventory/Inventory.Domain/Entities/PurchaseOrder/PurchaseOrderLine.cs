namespace Inventory.Domain.Entities.PurchaseOrder
{
    public class PurchaseOrderLine
    {
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal ReceivedQty { get; set; } = 0;

        public decimal UnitPrice { get; set; }

        public decimal LineTotal => OrderedQty * UnitPrice;
    }
}
