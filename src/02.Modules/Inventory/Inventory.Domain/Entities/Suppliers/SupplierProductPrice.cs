using SharedKernel.Entities;

namespace Inventory.Domain.Entities.Suppliers
{
    public class SupplierProductPrice : BaseEntity
    {
        public int SupplierId { get; set; }
        public int ProductId { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime EffectiveDate { get; set; }
    }
}
