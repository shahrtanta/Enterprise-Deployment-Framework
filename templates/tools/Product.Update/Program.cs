namespace Product.Update;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var manifest = Get(args, "--manifest")
                ?? throw new InvalidOperationException("--manifest is required.");

            var installDirectory = Get(args, "--install-directory")
                ?? AppContext.BaseDirectory;

            var workingDirectory = Get(args, "--working-directory")
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Company",
                    "Product",
                    "Updates");

            await new UpdateEngine(installDirectory, workingDirectory).ApplyAsync(
                manifest,
                args.Contains("--confirm-apply", StringComparer.OrdinalIgnoreCase));

            Console.WriteLine("Update applied successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Update failed: {ex.Message}");
            return 2;
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
