namespace Inventory.Application.DTOs.TrialBalances
{
    public class TrialBalanceDto
    {
        public int AccountId { get; set; }

        public string AccountCode { get; set; }

        public string AccountName { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal PeriodDebit { get; set; }

        public decimal PeriodCredit { get; set; }

        public decimal EndingBalance { get; set; }
    }
}
