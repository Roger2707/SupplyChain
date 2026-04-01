using Inventory.Domain.Enums;
using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Delivery
{
    public class Delivery : BaseEntity
    {
        public string OrderNumber { get; set; }
        public int SalesOrderId { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.Draft;

        public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; } = 0;

        public List<DeliveryLine> Lines { get; set; } = new();
    }
}
