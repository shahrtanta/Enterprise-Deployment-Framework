using Microsoft.Data.SqlClient;

namespace Product.BackupRestore;

internal sealed class SqlBackupService
{
    private readonly BackupOptions _options;

    public SqlBackupService(BackupOptions options)
    {
        _options = options;
    }

    public async Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        ValidateBaseOptions();
        Directory.CreateDirectory(_options.BackupDirectory);

        var databaseName = ResolveDatabaseName();
        var backupPath = Path.Combine(
            _options.BackupDirectory,
            $"{SanitizeFileName(databaseName)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.bak");

        var commandText = $"""
            BACKUP DATABASE {QuoteIdentifier(databaseName)}
            TO DISK = @backupPath
            WITH COPY_ONLY, CHECKSUM, INIT, STATS = 10;
            """;

        await ExecuteMasterCommandAsync(commandText, backupPath, cancellationToken);
        await ValidateBackupAsync(backupPath, cancellationToken);
        ApplyRetention();

        return backupPath;
    }

    public async Task ValidateBackupAsync(
        string backupPath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException("Backup file was not found.", backupPath);

        const string commandText = """
            RESTORE VERIFYONLY
            FROM DISK = @backupPath
            WITH CHECKSUM;
            """;

        await ExecuteMasterCommandAsync(commandText, backupPath, cancellationToken);
    }

    public async Task RestoreBackupAsync(
        string backupPath,
        CancellationToken cancellationToken = default)
    {
        ValidateBaseOptions();

        if (!_options.ConfirmRestore)
        {
            throw new InvalidOperationException(
                "Restore requires the explicit --confirm-restore argument.");
        }

        await ValidateBackupAsync(backupPath, cancellationToken);
        var databaseName = ResolveDatabaseName();

        var commandText = $"""
            ALTER DATABASE {QuoteIdentifier(databaseName)}
            SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

            RESTORE DATABASE {QuoteIdentifier(databaseName)}
            FROM DISK = @backupPath
            WITH REPLACE, CHECKSUM, STATS = 10;

            ALTER DATABASE {QuoteIdentifier(databaseName)}
            SET MULTI_USER;
            """;

        try
        {
            await ExecuteMasterCommandAsync(commandText, backupPath, cancellationToken);
        }
        catch
        {
            await TryReturnToMultiUserAsync(databaseName, cancellationToken);
            throw;
        }
    }

    private async Task ExecuteMasterCommandAsync(
        string commandText,
        string backupPath,
        CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(_options.ConnectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = Math.Min(
                new SqlConnectionStringBuilder(_options.ConnectionString).ConnectTimeout,
                15)
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.CommandTimeout = 0;
        command.Parameters.AddWithValue("@backupPath", backupPath);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private string ResolveDatabaseName()
    {
        if (!string.IsNullOrWhiteSpace(_options.DatabaseName))
            return _options.DatabaseName;

        var builder = new SqlConnectionStringBuilder(_options.ConnectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            throw new InvalidOperationException("Database name could not be resolved.");

        return builder.InitialCatalog;
    }

    private void ApplyRetention()
    {
        var cutoff = DateTime.UtcNow.AddDays(-_options.RetentionDays);
        var files = Directory.GetFiles(_options.BackupDirectory, "*.bak")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToList();

        // Never remove the newest valid-looking backup.
        foreach (var file in files.Skip(1).Where(file => file.LastWriteTimeUtc < cutoff))
        {
            try { file.Delete(); } catch { }
        }
    }

    private async Task TryReturnToMultiUserAsync(
        string databaseName,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = $"""
                IF DB_ID(@databaseName) IS NOT NULL
                BEGIN
                    DECLARE @sql nvarchar(max) =
                        N'ALTER DATABASE ' + QUOTENAME(@databaseName) +
                        N' SET MULTI_USER WITH ROLLBACK IMMEDIATE';
                    EXEC sp_executesql @sql;
                END
                """;

            var builder = new SqlConnectionStringBuilder(_options.ConnectionString)
            {
                InitialCatalog = "master"
            };

            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Parameters.AddWithValue("@databaseName", databaseName);
            await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            // Best-effort recovery only.
        }
    }

    private void ValidateBaseOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("A connection string is required.");
    }

    private static string QuoteIdentifier(string value)
        => $"[{value.Replace("]", "]]", StringComparison.Ordinal)}]";

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }
}
