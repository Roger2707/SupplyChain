namespace Inventory.Application.DTOs.Reports
{
    public class GeneralLedgerDto
    {
        public DateTime PostingDate { get; set; }

        public string Reference { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal Balance { get; set; }
    }
}
