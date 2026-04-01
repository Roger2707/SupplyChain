using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Suppliers
{
    public class Supplier : BaseEntity
    {
        public required string SupplierCode { get; set; }
        public required string SupplierName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public string Description { get; set; }
    }
}
