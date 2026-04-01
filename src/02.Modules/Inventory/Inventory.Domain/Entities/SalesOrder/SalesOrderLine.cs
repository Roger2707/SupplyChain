using Inventory.Domain.Entities.Products;

namespace Inventory.Domain.Entities.SalesOrder
{
    public class SalesOrderLine
    {
        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int RowNumber { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal DeliveredQty { get; set; } = 0;
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RemainingQty => OrderedQty - DeliveredQty;
        public decimal LineTotal => OrderedQty * UnitPrice;
    }
}
