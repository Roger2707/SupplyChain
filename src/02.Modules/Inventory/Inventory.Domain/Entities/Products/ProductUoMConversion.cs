namespace Inventory.Domain.Entities.Products
{
    public class ProductUoMConversion
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int FromUoMId { get; set; }
        public UoM FromUoM { get; set; } = default!;

        public int ToUoMId { get; set; }
        public UoM ToUoM { get; set; } = default!;

        public decimal Factor { get; set; }
    }
}
