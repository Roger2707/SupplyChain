using Inventory.Domain.Entities.Products;

namespace Inventory.Domain.Entities.Delivery
{
    public class DeliveryLine
    {
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int RowNumber { get; set; }
        public int? CostLayerId { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal InvoicedQty { get; set; } = 0;
        public decimal UnitCost { get; set; } = 0;
        public decimal RemainingInvoicedQty => DeliveredQty - InvoicedQty;
        public decimal LineTotal => DeliveredQty * UnitCost;
    }
}