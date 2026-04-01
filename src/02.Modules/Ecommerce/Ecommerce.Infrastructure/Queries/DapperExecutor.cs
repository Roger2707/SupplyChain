using ECommerce.Infrastructure.Data;
using SharedKernel.Repositories;

namespace ECommerce.Infrastructure.Queries;

public sealed class DapperExecutor : EfDapperExecutorBase<ECommerceDbContext>, IDapperExecutor
{
    public DapperExecutor(ECommerceDbContext db)
        : base(db)
    {
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        var result = await base.QueryAsync<T>(sql, param, cancellationToken: cancellationToken);
        return result.ToList().AsReadOnly();
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        return await base.QueryFirstOrDefaultAsync<T>(sql, param, cancellationToken: cancellationToken);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        return await base.ExecuteAsync(sql, param, cancellationToken: cancellationToken);
    }
}
