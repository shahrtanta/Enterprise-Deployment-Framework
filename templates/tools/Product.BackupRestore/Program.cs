namespace Product.BackupRestore;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = BackupOptions.Parse(args);
            var service = new SqlBackupService(options);

            switch (options.Operation)
            {
                case BackupOperation.Backup:
                    Console.WriteLine(
                        $"Backup created: {await service.CreateBackupAsync()}");
                    return 0;

                case BackupOperation.Validate:
                    if (string.IsNullOrWhiteSpace(options.BackupFile))
                        throw new InvalidOperationException("--backup-file is required.");

                    await service.ValidateBackupAsync(options.BackupFile);
                    Console.WriteLine("Backup validation succeeded.");
                    return 0;

                case BackupOperation.Restore:
                    if (string.IsNullOrWhiteSpace(options.BackupFile))
                        throw new InvalidOperationException("--backup-file is required.");

                    await service.RestoreBackupAsync(options.BackupFile);
                    Console.WriteLine("Database restore succeeded.");
                    return 0;

                default:
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Backup/restore failed: {Sanitize(ex.Message)}");
            return 2;
        }
    }

    private static string Sanitize(string value)
    {
        foreach (var marker in new[] { "Password=", "Pwd=" })
        {
            var start = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0) continue;
            var end = value.IndexOf(';', start);
            value = end >= 0
                ? value[..start] + marker + "***" + value[end..]
                : value[..start] + marker + "***";
        }
        return value;
    }
}
