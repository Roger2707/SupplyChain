namespace Inventory.Application.DTOs.SalesOrder
{
    public class UpdateSalesOrderDto
    {
        public int CustomerId { get; set; }

        // Concurrency token from client (taken from SalesOrderDto.RowVersion)
        public byte[]? RowVersion { get; set; }

        public List<UpdateSalesOrderLineDto> UpdateLinesDto { get; set; } = new();
    }

    public class UpdateSalesOrderLineDto
    {
        public int ProductId { get; set; }
        public decimal OrderedQty { get; set; }
    }
}
