using Microsoft.Data.SqlClient;

namespace Product.Reference.Web.Services;

public sealed class DatabaseHealthService
{
    public async Task<(bool Success, string Message)> TestAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT DB_NAME();";
            command.CommandTimeout = 8;

            var database = (string?)await command.ExecuteScalarAsync(cancellationToken);

            return (
                true,
                $"Connection established successfully to '{database ?? "(unknown)"}'.");
        }
        catch (SqlException ex)
        {
            return (
                false,
                $"Database connection failed. SQL error {ex.Number}.");
        }
        catch (Exception ex)
        {
            return (
                false,
                $"Database connection failed: {ex.Message}");
        }
    }
}
