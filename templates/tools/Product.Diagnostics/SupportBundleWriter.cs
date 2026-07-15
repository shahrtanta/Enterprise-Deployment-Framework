using System.IO.Compression;
using System.Text.Json;

namespace Product.Diagnostics;

internal static class SupportBundleWriter
{
    public static async Task<string> WriteAsync(
        DiagnosticOptions options,
        DiagnosticReport report,
        string outputDirectory,
        CancellationToken token = default)
    {
        Directory.CreateDirectory(outputDirectory);
        var work = Path.Combine(Path.GetTempPath(), $"ProductSupport-{Guid.NewGuid():N}");
        Directory.CreateDirectory(work);

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(work, "diagnostic-report.json"),
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }),
                token);

            await File.WriteAllLinesAsync(
                Path.Combine(work, "diagnostic-summary.txt"),
                report.Findings.Select(f => $"[{f.Status}] {f.Category}/{f.Check}: {f.Message}"),
                token);

            CopyRecentLogs(options, work);

            var zip = Path.Combine(
                outputDirectory,
                $"{options.ApplicationName}-Support-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip");

            ZipFile.CreateFromDirectory(work, zip, CompressionLevel.Optimal, false);
            return zip;
        }
        finally
        {
            try { Directory.Delete(work, true); } catch { }
        }
    }

    private static void CopyRecentLogs(DiagnosticOptions options, string work)
    {
        var source = Path.Combine(options.DataRoot, "Logs");
        if (!Directory.Exists(source)) return;

        var destination = Path.Combine(work, "logs");
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(source, "*.log")
                     .Select(path => new FileInfo(path))
                     .OrderByDescending(file => file.LastWriteTimeUtc)
                     .Take(10))
        {
            File.Copy(file.FullName, Path.Combine(destination, file.Name), true);
        }
    }
}
