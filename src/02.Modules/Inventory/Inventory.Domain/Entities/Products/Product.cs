using SharedKernel.Entities;
namespace Inventory.Domain.Entities.Products
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string SKU { get; set; }
        public string Barcode { get; set; }

        // Category
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        // Base Unit
        public int BaseUoMId { get; set; }
        public UoM BaseUoM { get; set; } = default!;

        public decimal MinStockLevel { get; set; }
        public decimal SellingPrice { get; set; }

        public bool IsPerishable { get; set; } = true;
        public bool IsActive { get; set; } = true;

        // Conversion navigation
        public ICollection<ProductUoMConversion> Conversions { get; set; }
            = new List<ProductUoMConversion>();
    }
}
