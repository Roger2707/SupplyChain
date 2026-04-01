using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Products
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
    }
}
