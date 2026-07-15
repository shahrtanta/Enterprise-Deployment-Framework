using System.Security.Cryptography;
using Microsoft.Data.SqlClient;

namespace Product.MigrationRunner;

internal sealed class MigrationEngine
{
    private readonly string _connectionString;
    private readonly string _scriptsDirectory;

    public MigrationEngine(string connectionString, string scriptsDirectory)
    {
        _connectionString = connectionString;
        _scriptsDirectory = scriptsDirectory;
    }

    public async Task<IReadOnlyList<MigrationResult>> ApplyAsync(
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_scriptsDirectory))
            throw new DirectoryNotFoundException(_scriptsDirectory);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await EnsureHistoryTableAsync(connection, cancellationToken);
        var applied = await ReadAppliedAsync(connection, cancellationToken);
        var scripts = DiscoverScripts();

        var results = new List<MigrationResult>();

        foreach (var script in scripts)
        {
            if (applied.TryGetValue(script.Version, out var existingHash))
            {
                if (!string.Equals(existingHash, script.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new MigrationResult(
                        script.Version,
                        false,
                        "Applied migration hash does not match the current script."));
                    break;
                }

                results.Add(new MigrationResult(
                    script.Version,
                    true,
                    "Already applied."));
                continue;
            }

            try
            {
                var sql = await File.ReadAllTextAsync(script.Path, cancellationToken);
                await using var transaction =
                    (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

                try
                {
                    await using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = sql;
                    command.CommandTimeout = 0;
                    await command.ExecuteNonQueryAsync(cancellationToken);

                    await using var historyCommand = connection.CreateCommand();
                    historyCommand.Transaction = transaction;
                    historyCommand.CommandText = """
                        INSERT INTO dbo.__DeploymentMigrationHistory
                            (Version, Sha256, AppliedAtUtc)
                        VALUES
                            (@version, @sha256, SYSUTCDATETIME());
                        """;
                    historyCommand.Parameters.AddWithValue("@version", script.Version);
                    historyCommand.Parameters.AddWithValue("@sha256", script.Sha256);
                    await historyCommand.ExecuteNonQueryAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    results.Add(new MigrationResult(
                        script.Version,
                        true,
                        "Applied successfully."));
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                results.Add(new MigrationResult(
                    script.Version,
                    false,
                    ex.Message));
                break;
            }
        }

        return results;
    }

    private IReadOnlyList<MigrationScript> DiscoverScripts()
    {
        return Directory.GetFiles(_scriptsDirectory, "*.sql")
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .Select(path => new MigrationScript(
                Path.GetFileNameWithoutExtension(path),
                path,
                ComputeSha256(path)))
            .ToList();
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static async Task EnsureHistoryTableAsync(
        SqlConnection connection,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID(N'dbo.__DeploymentMigrationHistory', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.__DeploymentMigrationHistory
                (
                    Version nvarchar(200) NOT NULL PRIMARY KEY,
                    Sha256 char(64) NOT NULL,
                    AppliedAtUtc datetime2(0) NOT NULL
                );
            END
            """;
        await command.ExecuteNonQueryAsync(token);
    }

    private static async Task<Dictionary<string, string>> ReadAppliedAsync(
        SqlConnection connection,
        CancellationToken token)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Version, Sha256
            FROM dbo.__DeploymentMigrationHistory;
            """;

        await using var reader = await command.ExecuteReaderAsync(token);
        while (await reader.ReadAsync(token))
        {
            result[reader.GetString(0)] = reader.GetString(1);
        }

        return result;
    }
}
