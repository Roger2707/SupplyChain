using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.DTOs.Warehouses;

public class UpdateWarehouseDto
{
    public byte[]? RowVersion { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string WarehouseCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string WarehouseName { get; set; } = string.Empty;

    [StringLength(255)]
    public string Address { get; set; }

    [StringLength(20)]
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string Description { get; set; }
}

