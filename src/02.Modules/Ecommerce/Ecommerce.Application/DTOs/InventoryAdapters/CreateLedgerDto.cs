namespace ECommerce.Application.DTOs.InventoryAdapters
{
    public class CreateLedgerDto
    {
        public int ProductId { get; set; }

        public int WarehouseId { get; set; }

        public int ReferenceId { get; set; }

        public string ReferenceType { get; set; } = "Orders";

        public decimal QuantityIn { get; set; } = 0;

        public decimal QuantityOut { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalCost { get; set; }
    }
}
