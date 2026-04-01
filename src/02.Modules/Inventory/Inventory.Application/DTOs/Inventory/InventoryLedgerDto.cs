using Inventory.Domain.Enums;

namespace Inventory.Application.DTOs.Inventory
{
    public class InventoryLedgerDto
    {
        public int ProductId { get; set; }

        public int WarehouseId { get; set; }

        public InventoryTransactionType TransactionType { get; set; }

        public int ReferenceId { get; set; }

        public string ReferenceType { get; set; }

        public decimal QuantityIn { get; set; }

        public decimal QuantityOut { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalCost { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
