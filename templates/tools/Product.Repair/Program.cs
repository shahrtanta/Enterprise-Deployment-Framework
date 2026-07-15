namespace Product.Repair;

internal sealed record RepairResult(string Action, bool Success, string Message);

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var company = Get(args, "--company") ?? "Company";
            var app = Get(args, "--application") ?? "Product";
            var dataRoot = Get(args, "--data-root")
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    company,
                    app);

            var dryRun = Has(args, "--dry-run");
            var results = new List<RepairResult>();

            if (Has(args, "--repair-folders"))
                RepairFolders(dataRoot, dryRun, results);

            if (Has(args, "--repair-configuration"))
                RepairConfiguration(args, dataRoot, dryRun, results);

            if (Has(args, "--clear-temp"))
                ClearTemp(dataRoot, dryRun, results);

            if (results.Count == 0)
                results.Add(new("No operation", false, "No repair action was selected."));

            foreach (var result in results)
                Console.WriteLine($"[{(result.Success ? "SUCCESS" : "FAILED")}] {result.Action}: {result.Message}");

            WriteReport(dataRoot, app, dryRun, results);
            return results.All(result => result.Success) ? 0 : 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Repair failed: {ex.Message}");
            return 3;
        }
    }

    private static void RepairFolders(string dataRoot, bool dryRun, List<RepairResult> results)
    {
        foreach (var name in new[]
                 {
                     "Config", "Data", "Database", "Backups", "Logs", "Reports",
                     "Temp", "Uploads", "Exports", "Imports", "Updates"
                 })
        {
            var path = Path.Combine(dataRoot, name);
            try
            {
                if (!dryRun) Directory.CreateDirectory(path);
                results.Add(new($"Create directory {name}", true,
                    dryRun ? $"Would create or verify: {path}" : $"Available: {path}"));
            }
            catch (Exception ex)
            {
                results.Add(new($"Create directory {name}", false, ex.Message));
            }
        }
    }

    private static void RepairConfiguration(
        string[] args,
        string dataRoot,
        bool dryRun,
        List<RepairResult> results)
    {
        var runtime = Get(args, "--runtime-configuration")
            ?? Path.Combine(dataRoot, "Config", "appsettings.runtime.json");
        var template = Get(args, "--default-configuration")
            ?? Path.Combine(AppContext.BaseDirectory, "appsettings.Production.example.json");

        try
        {
            if (File.Exists(runtime))
            {
                results.Add(new("Runtime configuration", true,
                    "Existing runtime configuration was preserved."));
                return;
            }

            if (!File.Exists(template))
            {
                results.Add(new("Runtime configuration", false,
                    $"Default template not found: {template}"));
                return;
            }

            if (!dryRun)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(runtime)!);
                File.Copy(template, runtime, false);
            }

            results.Add(new("Runtime configuration", true,
                dryRun ? $"Would copy template to {runtime}" : $"Restored to {runtime}"));
        }
        catch (Exception ex)
        {
            results.Add(new("Runtime configuration", false, ex.Message));
        }
    }

    private static void ClearTemp(string dataRoot, bool dryRun, List<RepairResult> results)
    {
        var temp = Path.Combine(dataRoot, "Temp");
        try
        {
            if (!Directory.Exists(temp))
            {
                results.Add(new("Clear temporary files", true, "Temporary directory does not exist."));
                return;
            }

            var cutoff = DateTime.UtcNow.AddDays(-2);
            var candidates = Directory.GetFiles(temp, "*", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .Where(file => file.LastWriteTimeUtc < cutoff)
                .ToList();

            if (!dryRun)
                foreach (var file in candidates) file.Delete();

            results.Add(new("Clear temporary files", true,
                dryRun
                    ? $"Would delete {candidates.Count} stale file(s)."
                    : $"Deleted {candidates.Count} stale file(s)."));
        }
        catch (Exception ex)
        {
            results.Add(new("Clear temporary files", false, ex.Message));
        }
    }

    private static void WriteReport(
        string dataRoot,
        string app,
        bool dryRun,
        IReadOnlyList<RepairResult> results)
    {
        if (dryRun) return;

        try
        {
            var reports = Path.Combine(dataRoot, "Reports");
            Directory.CreateDirectory(reports);
            File.WriteAllLines(
                Path.Combine(reports, $"repair-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt"),
                new[]
                {
                    $"Repair report: {app}",
                    $"Generated UTC: {DateTime.UtcNow:O}",
                    ""
                }.Concat(results.Select(
                    result => $"[{(result.Success ? "SUCCESS" : "FAILED")}] {result.Action}: {result.Message}")));
        }
        catch { }
    }

    private static bool Has(string[] args, string name)
        => args.Contains(name, StringComparer.OrdinalIgnoreCase);

    private static string? Get(string[] args, string name)
    {
        var index = Array.FindIndex(
            args,
            value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
