using Microsoft.Data.SqlClient;
using Product.Deployment.Core;

namespace Product.DbConnectionTester;

internal static class SqlConnectionTester
{
    public static async Task<ConnectionTestResult> TestAsync(ConnectionTestRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(ConnectionStringFactory.Build(request));
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT DB_NAME(), CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(128));";
            command.CommandTimeout = request.TimeoutSeconds;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            string? database = null, version = null;
            if (await reader.ReadAsync(cancellationToken))
            {
                database = reader.IsDBNull(0) ? null : reader.GetString(0);
                version = reader.IsDBNull(1) ? null : reader.GetString(1);
            }
            return new(true, "Connection established successfully.", version, database);
        }
        catch (SqlException ex)
        {
            return new(false, "Database connection failed. Check the server, instance, authentication, and network availability.", ErrorCode: ex.Number.ToString());
        }
        catch (Exception ex)
        {
            return new(false, ex.Message, ErrorCode: ex.GetType().Name);
        }
    }
}
