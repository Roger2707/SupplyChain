namespace Inventory.Application.DTOs.Reports
{
    public class GeneralLedgerRequest
    {
        public int AccountId { get; set; }
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
