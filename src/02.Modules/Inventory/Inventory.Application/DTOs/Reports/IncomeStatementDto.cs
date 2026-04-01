namespace Inventory.Application.DTOs.Reports
{
    public class IncomeStatementDto
    {
        public int AccountId { get; set; }

        public string AccountCode { get; set; }

        public string AccountName { get; set; }

        public string AccountType { get; set; }

        public decimal Amount { get; set; }
    }
}
