namespace Identity.Domain.Entities
{
    public class UserWarehouse
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public bool IsWarehouseManager { get; set; } = false;
    }
}