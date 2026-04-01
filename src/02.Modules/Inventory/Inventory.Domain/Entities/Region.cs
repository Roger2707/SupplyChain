using SharedKernel.Entities;

namespace Inventory.Domain.Entities
{
    public class Region : BaseEntity
    {
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
    }
}
