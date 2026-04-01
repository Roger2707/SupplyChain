namespace Inventory.Application.DTOs.Products
{
    public class CreateProductDto
    {
        public string Name { get; init; }
        public int CategoryId { get; set; }
        public int BaseUoMId { get; set; }
        public decimal MinStockLevel { get; set; }
        public List<CreateProductUOMConversionDto>? ConversionDtos { get; set; } = new();
    }

    public class CreateProductUOMConversionDto
    {
        public int FromUoMId { get; set; }
        public int ToUoMId { get; set; }
        public decimal Factor { get; set; }
    }
}
