using Inventory.Application.DTOs.Reports;
using Inventory.Application.DTOs.TrialBalances;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<Result<IEnumerable<TrialBalanceDto>>> GetTrialBalanceAsync(TrialBalanceRequest request, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<GeneralLedgerDto>>> GetGeneralLedgerAsync(GeneralLedgerRequest request, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<IncomeStatementDto>>>GetIncomeStatementAsync(IncomeStatementRequest request, CancellationToken cancellationToken = default);
    }
}



