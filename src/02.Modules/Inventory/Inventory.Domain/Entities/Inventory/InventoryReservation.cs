using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Inventory
{
    public class InventoryReservation : BaseEntity
    {
        public int LayerId { get; set; }
        public int ProductId { get; set; }
        public int RowNumber { get; set; } = 0;
        public decimal ReservedQty { get; set; }
        public string SourceType { get; set; }
        public int SourceId { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalAmount => UnitCost * ReservedQty;
    }
}