using SharedKernel.Entities;

namespace Inventory.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public required string CustomerCode { get; set; }
        public required string CustomerName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}
