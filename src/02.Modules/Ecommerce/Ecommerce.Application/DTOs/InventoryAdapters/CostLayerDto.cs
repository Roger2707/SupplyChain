namespace ECommerce.Application.DTOs.InventoryAdapters
{
    public class CostLayerDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal OriginalQty { get; set; }
        public decimal RemainingQty { get; set; }
        public decimal ReservedQty { get; set; }
        public decimal UnitCost { get; set; }
    }
}
