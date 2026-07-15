using System.Text.Json;

namespace Product.Diagnostics;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = DiagnosticOptions.Load(
                AppContext.BaseDirectory,
                GetArgument(args, "--configuration"));

            var report = await new DiagnosticRunner(options).RunAsync();

            if (args.Contains("--json", StringComparer.OrdinalIgnoreCase))
                Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            else
                foreach (var finding in report.Findings)
                    Console.WriteLine($"[{finding.Status,-7}] {finding.Category}/{finding.Check}: {finding.Message}");

            if (args.Contains("--support-bundle", StringComparer.OrdinalIgnoreCase))
            {
                var output = GetArgument(args, "--output")
                    ?? Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        "Product Support");

                Console.WriteLine($"Support bundle: {await SupportBundleWriter.WriteAsync(options, report, output)}");
            }

            return report.Findings.Any(f => f.Status == DiagnosticStatus.Failed) ? 2 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Diagnostics failed: {ex.Message}");
            return 3;
        }
    }

    private static string? GetArgument(string[] args, string name)
    {
        var index = Array.FindIndex(
            args,
            value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));

        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
