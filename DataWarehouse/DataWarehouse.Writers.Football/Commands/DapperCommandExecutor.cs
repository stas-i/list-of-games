using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace DataWarehouse.Writers.Football.Commands;

public class DapperCommandExecutor : IDapperCommandExecutor
{
    private readonly string _connectionString;

    public DapperCommandExecutor(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Warehouse")
                           ?? throw new NullReferenceException("Warehouse ConnectionString was not found");
    }

    public async Task<int> Execute(IDapperCommand command, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.ExecuteAsync(command.Sql, command.Params, commandType: CommandType.Text);
    }
}

public interface IDapperCommandExecutor
{
    Task<int> Execute(IDapperCommand command, CancellationToken cancellationToken = default);
}

public interface IDapperCommand
{
    internal string Sql { get; }
    object Params { get; }
}
