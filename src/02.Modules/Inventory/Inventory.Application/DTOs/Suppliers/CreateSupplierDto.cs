using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.DTOs.Suppliers
{
    public class CreateSupplierDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
