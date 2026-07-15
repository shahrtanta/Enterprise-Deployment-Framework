namespace Product.MigrationRunner;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var connection = Get(args, "--connection-string")
                ?? Environment.GetEnvironmentVariable("PRODUCT_CONNECTION_STRING")
                ?? throw new InvalidOperationException("A connection string is required.");

            var scripts = Get(args, "--scripts")
                ?? throw new InvalidOperationException("--scripts is required.");

            var results = await new MigrationEngine(connection, scripts).ApplyAsync();

            foreach (var result in results)
            {
                Console.WriteLine(
                    $"[{(result.Success ? "SUCCESS" : "FAILED")}] {result.Version}: {result.Message}");
            }

            return results.All(result => result.Success) ? 0 : 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Migration failed: {ex.Message}");
            return 3;
        }
    }

    private static string? Get(string[] args, string name)
    {
        var index = Array.FindIndex(
            args,
            value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));

        return index >= 0 && index + 1 < args.Length
            ? args[index + 1]
            : null;
    }
}
