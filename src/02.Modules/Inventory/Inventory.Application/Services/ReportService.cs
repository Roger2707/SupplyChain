using Inventory.Application.DTOs.Reports;
using Inventory.Application.DTOs.TrialBalances;
using Inventory.Application.Interfaces.Queries;
using Inventory.Application.Interfaces.Services;
using SharedKernel.Entities;

namespace Inventory.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportQueries _reportQueries;

        public ReportService(IReportQueries reportQueries)
        {
            _reportQueries = reportQueries;
        }

        public async Task<Result<IEnumerable<TrialBalanceDto>>> GetTrialBalanceAsync(TrialBalanceRequest request, CancellationToken cancellationToken = default)
        {
            var trialBalances = await _reportQueries.GetTrialBalanceAsync(request, cancellationToken);
            return Result<IEnumerable<TrialBalanceDto>>.Success(trialBalances);
        }

        public async Task<Result<IEnumerable<GeneralLedgerDto>>> GetGeneralLedgerAsync(GeneralLedgerRequest request, CancellationToken cancellationToken = default)
        {
            var generalLedger = await _reportQueries.GetGeneralLedgerAsync(request, cancellationToken);
            return Result<IEnumerable<GeneralLedgerDto>>.Success(generalLedger);
        }

        public async Task<Result<IEnumerable<IncomeStatementDto>>> GetIncomeStatementAsync(IncomeStatementRequest request, CancellationToken cancellationToken = default)
        {
            var incomeStatements = await _reportQueries.GetIncomeStatementAsync(request, cancellationToken);
            return Result<IEnumerable<IncomeStatementDto>>.Success(incomeStatements);
        }
    }
}



