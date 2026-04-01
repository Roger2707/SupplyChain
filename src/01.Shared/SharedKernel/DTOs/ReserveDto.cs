namespace SharedKernel.DTOs
{
    public class ReserveDto
    {
        public int SourceId { get; set; }
        public int ProductId { get; set; }
        public int RowNumber { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ReservedQty { get; set; }
    }
}
