using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Inventory
{
    public class InventoryCostLayer : BaseEntity
    {
        public int GoodsReceiptId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }

        public decimal OriginalQty { get; set; }

        public decimal RemainingQty { get; set; }
        public decimal ReservedQty { get; set; }

        public decimal UnitCost { get; set; }

        public DateTime ReceiptDate { get; set; }

        public bool IsClosed => RemainingQty <= 0;
    }
}
