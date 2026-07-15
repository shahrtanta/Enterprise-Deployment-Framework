namespace Product.BackupRestore;

internal enum BackupOperation
{
    Backup,
    Restore,
    Validate
}

internal sealed record BackupOptions
{
    public BackupOperation Operation { get; init; }
    public string ConnectionString { get; init; } = string.Empty;
    public string BackupDirectory { get; init; } = string.Empty;
    public string? BackupFile { get; init; }
    public string DatabaseName { get; init; } = string.Empty;
    public int RetentionDays { get; init; } = 30;
    public bool ConfirmRestore { get; init; }

    public static BackupOptions Parse(string[] args)
    {
        string? Get(string name)
        {
            var index = Array.FindIndex(
                args,
                value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        bool Has(string name) => args.Contains(name, StringComparer.OrdinalIgnoreCase);

        var operation = Has("--restore")
            ? BackupOperation.Restore
            : Has("--validate")
                ? BackupOperation.Validate
                : BackupOperation.Backup;

        var retention = int.TryParse(Get("--retention-days"), out var parsed)
            ? parsed
            : 30;

        return new BackupOptions
        {
            Operation = operation,
            ConnectionString = Get("--connection-string")
                ?? Environment.GetEnvironmentVariable("PRODUCT_CONNECTION_STRING")
                ?? string.Empty,
            BackupDirectory = Get("--backup-directory")
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Company",
                    "Product",
                    "Backups"),
            BackupFile = Get("--backup-file"),
            DatabaseName = Get("--database-name") ?? string.Empty,
            RetentionDays = Math.Max(1, retention),
            ConfirmRestore = Has("--confirm-restore")
        };
    }
}
