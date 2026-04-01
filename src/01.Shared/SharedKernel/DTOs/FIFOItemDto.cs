namespace SharedKernel.DTOs
{
    public class FIFOItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal NeccessaryQty { get; set; }
        public int SourceId { get; set; }
        public string SourceType { get; set; }
    }
}
