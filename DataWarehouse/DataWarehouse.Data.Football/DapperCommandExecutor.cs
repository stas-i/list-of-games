using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace DataWarehouse.Data.Football;

public class DapperExecutor : IDapperExecutor
{
    private readonly string _connectionString;

    public DapperExecutor(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Warehouse")
                           ?? throw new NullReferenceException("Warehouse ConnectionString was not found");
    }

    public async Task<IList<T>> QueryAsync<T>(IDapperParams<T> command, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var result = await connection.QueryAsync<T>(command.Sql, command.Params, commandType: CommandType.Text);

        return result?.ToList() ?? new List<T>();
    }

    public async Task<T> QuerySingle<T>(IDapperParams<T> command, CancellationToken cancellationToken = default)
    {
        var list = await QueryAsync(command, cancellationToken);
        return list.Single();
    }

    public async Task<T?> QuerySingleOrDefault<T>(IDapperParams<T> command, CancellationToken cancellationToken = default)
    {
        var list = await QueryAsync(command, cancellationToken);
        return list.SingleOrDefault();
    }
}

public interface IDapperExecutor
{
    Task<IList<T>> QueryAsync<T>(IDapperParams<T> dapperParams, CancellationToken cancellationToken = default);
    Task<T> QuerySingle<T>(IDapperParams<T> dapperParams, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleOrDefault<T>(IDapperParams<T> dapperParams, CancellationToken cancellationToken = default);
}

public interface IDapperCommand<T> : IDapperParams<T>
{
}
public interface IDapperQuery<T> : IDapperParams<T>
{
}

public interface IDapperParams<T>
{
    internal string Sql { get; }
    object Params { get; }
}
