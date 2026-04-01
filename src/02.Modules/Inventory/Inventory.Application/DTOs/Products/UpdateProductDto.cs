namespace Inventory.Application.DTOs.Products
{
    public class UpdateProductDto
    {
        public byte[]? RowVersion { get; set; }

        public string Name { get; init; }
        public int CategoryId { get; set; }
        public int BaseUoMId { get; set; }
        public decimal MinStockLevel { get; set; }
        public List<UpdateProductUOMConversionDto>? ConversionDtos { get; set; } = new();
    }

    public class UpdateProductUOMConversionDto
    {
        public int FromUoMId { get; set; }
        public int ToUoMId { get; set; }
        public decimal Factor { get; set; }
    }
}
