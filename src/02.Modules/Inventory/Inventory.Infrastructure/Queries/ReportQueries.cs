using Inventory.Application.DTOs.Reports;
using Inventory.Application.DTOs.TrialBalances;
using Inventory.Application.Interfaces.Queries;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Queries
{
    public class ReportQueries : IReportQueries
    {
        private readonly IDapperExecutor _dapper;

        public ReportQueries(IDapperExecutor dapper)
        {
            _dapper = dapper;
        }

        public async Task<IEnumerable<TrialBalanceDto>> GetTrialBalanceAsync(TrialBalanceRequest request, CancellationToken cancellationToken = default)
        {
            const string sql = """
                           SELECT
                           	account.Id as AccountId
                           	, account.Code as AccountCode
                           	, account.Name as AccountName
                           	, SUM(IIF(je.PostingDate < @FromDate, jel.Debit - jel.Credit, 0)) as OpeningBalance
                           	, SUM(IIF(je.PostingDate BETWEEN @FromDate AND @ToDate, jel.Debit, 0)) as PeriodDebit
                           	, SUM(IIF(je.PostingDate BETWEEN @FromDate AND @ToDate, jel.Credit, 0)) as PeriodCredit	
                           	, SUM(IIF(je.PostingDate <= @ToDate, jel.Debit - jel.Credit, 0)) AS EndingBalance

                           FROM JournalEntries je

                           INNER JOIN JournalEntryLines jel
                           ON jel.JournalEntryId = je.Id

                           INNER JOIN Accounts account 
                           ON account.Id = jel.AccountId

                           GROUP BY account.Id , account.Code , account.Name 
                           """;

            var rows = await _dapper.QueryAsync<TrialBalanceDto>(sql, 
                new {FromDate = request.FromDate, ToDate = request.ToDate}, 
                cancellationToken: cancellationToken);

            return rows;
        }

        public async Task<IEnumerable<GeneralLedgerDto>> GetGeneralLedgerAsync(GeneralLedgerRequest request, CancellationToken cancellationToken = default)
        {
            const string sql = """
                           DECLARE @OpeningBalance DECIMAL(18,2)
                           SELECT 
                               @OpeningBalance = SUM(jel.Debit - jel.Credit)
                           FROM JournalEntries je
                           JOIN JournalEntryLines jel ON jel.JournalEntryId = je.Id
                           WHERE jel.AccountId = @AccountId
                           AND je.PostingDate < @FromDate

                           SELECT
                               je.PostingDate,
                               je.Reference,
                               jel.Debit,
                               jel.Credit,

                               @OpeningBalance +
                               SUM(jel.Debit - jel.Credit)
                               OVER (ORDER BY je.PostingDate, je.Id) AS Balance

                           FROM JournalEntries je

                           JOIN JournalEntryLines jel
                           ON jel.JournalEntryId = je.Id

                           WHERE jel.AccountId = @AccountId
                           AND je.PostingDate BETWEEN @FromDate AND @ToDate

                           ORDER BY je.PostingDate
                           """;

            var rows = await _dapper.QueryAsync<GeneralLedgerDto>(sql,
                new { AccountId = request.AccountId, FromDate = request.FromDate, ToDate = request.ToDate },
                cancellationToken: cancellationToken);

            return rows;
        }

        public async Task<IEnumerable<IncomeStatementDto>> GetIncomeStatementAsync(IncomeStatementRequest request, CancellationToken cancellationToken = default)
        {
            const string sql = """
                           SELECT
                               a.Id AS AccountId,
                               a.Code AS AccountCode,
                               a.Name AS AccountName,
                               a.Type AS AccountType,

                               SUM(jel.Debit) AS TotalDebit,
                               SUM(jel.Credit) AS TotalCredit,

                               CASE 
                                   WHEN a.Type = 4 THEN SUM(jel.Credit - jel.Debit)
                                   WHEN a.Type = 5 THEN SUM(jel.Debit - jel.Credit)
                               END as Amount

                           FROM JournalEntries je

                           JOIN JournalEntryLines jel
                           ON jel.JournalEntryId = je.Id

                           JOIN Accounts a
                           ON a.Id = jel.AccountId

                           WHERE je.PostingDate BETWEEN @FromDate AND @ToDate
                           AND a.Type IN (4,5)

                           GROUP BY
                               a.Id,
                               a.Code,
                               a.Name,
                               a.Type

                           ORDER BY a.Code
                           """;

            var rows = await _dapper.QueryAsync<IncomeStatementDto>(sql,
                new { FromDate = request.FromDate, ToDate = request.ToDate },
                cancellationToken: cancellationToken);

            return rows;
        }
    }
}
