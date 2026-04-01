using Microsoft.EntityFrameworkCore;
using System.Data;
using Dapper;

namespace SharedKernel.Repositories;

public interface IDapperExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}

public class EfDapperExecutorBase<TContext> : IDapperExecutor where TContext : DbContext
{
    protected readonly TContext Context;

    public EfDapperExecutorBase(TContext context)
    {
        Context = context;
    }

    public async Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
        return connection;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql, 
        object? parameters = null, 
        CommandType commandType = CommandType.Text, 
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        return await connection.QueryAsync<T>(sql, parameters, null, commandType: commandType);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql, 
        object? parameters = null, 
        CommandType commandType = CommandType.Text, 
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, null, commandType: commandType);
    }

    public async Task<T?> QuerySingleAsync<T>(
        string sql, 
        object? parameters = null, 
        CommandType commandType = CommandType.Text, 
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, null, commandType: commandType);
    }

    public async Task<int> ExecuteAsync(
        string sql, 
        object? parameters = null, 
        CommandType commandType = CommandType.Text, 
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(sql, parameters, null, commandType: commandType);
    }
}
