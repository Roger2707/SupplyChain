using Inventory.Application.DTOs.Reports;
using Inventory.Application.DTOs.TrialBalances;

namespace Inventory.Application.Interfaces.Queries
{
    public interface IReportQueries
    {
        Task<IEnumerable<TrialBalanceDto>> GetTrialBalanceAsync(TrialBalanceRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<GeneralLedgerDto>> GetGeneralLedgerAsync(GeneralLedgerRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<IncomeStatementDto>> GetIncomeStatementAsync(IncomeStatementRequest request, CancellationToken cancellationToken = default);
    }
}
