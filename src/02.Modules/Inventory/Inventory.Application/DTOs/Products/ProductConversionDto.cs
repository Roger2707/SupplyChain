namespace Inventory.Application.DTOs.Products
{
    public class ProductConversionDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int UoMIdFrom { get; set; }
        public string UoMNameFrom { get; set; }
        public int UoMIdTo { get; set; }
        public string UoMNameTo { get; set; }
        public decimal Factor { get; set; }
    }
}
