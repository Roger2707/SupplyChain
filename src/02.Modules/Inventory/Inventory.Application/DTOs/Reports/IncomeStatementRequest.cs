namespace Inventory.Application.DTOs.Reports
{
    public class IncomeStatementRequest
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
