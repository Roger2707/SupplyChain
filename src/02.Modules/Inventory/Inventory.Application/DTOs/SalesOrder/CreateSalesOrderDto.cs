namespace Inventory.Application.DTOs.SalesOrder
{
    public class CreateSalesOrderDto
    {
        public int CustomerId { get; set; }
        public List<CreateSalesOrderLineDto> CreateLinesDto { get; set; } = new();
    }

    public class CreateSalesOrderLineDto
    {
        public int ProductId { get; set; }
        public decimal OrderedQty { get; set; }
    }
}