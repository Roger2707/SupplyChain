using SharedKernel.Entities;

namespace Inventory.Domain.Entities;

public class Warehouse : BaseEntity
{
    public required string WarehouseCode { get; set; }

    public required string WarehouseName { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
    public int? RegionId { get; set; }
    public Region Region { get; set; }
}


