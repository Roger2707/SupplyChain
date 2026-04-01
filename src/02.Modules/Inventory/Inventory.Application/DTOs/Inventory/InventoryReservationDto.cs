namespace Inventory.Application.DTOs.Inventory
{
    public class InventoryReservationDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal ReservedQty { get; set; }
        public string SourceType { get; set; }
        public int SourceId { get; set; }
    }
}
