using SharedKernel.Entities;

namespace ECommerce.Domain.Entities.Orders
{
    public  class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        // Snapshot for product
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}