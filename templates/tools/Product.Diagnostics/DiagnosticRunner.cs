using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Product.Diagnostics;

internal sealed class DiagnosticRunner
{
    private readonly DiagnosticOptions _options;

    public DiagnosticRunner(DiagnosticOptions options) => _options = options;

    public async Task<DiagnosticReport> RunAsync(CancellationToken cancellationToken = default)
    {
        var findings = new List<DiagnosticFinding>();
        CheckSystem(findings);
        CheckDirectories(findings);
        CheckConfiguration(findings);
        CheckPort(findings);
        await CheckHealthAsync(findings, cancellationToken);
        await CheckDatabaseAsync(findings, cancellationToken);
        CheckDisk(findings);
        CheckLogs(findings);

        return new DiagnosticReport(
            _options.ApplicationName,
            _options.ApplicationVersion,
            DateTimeOffset.UtcNow,
            Environment.MachineName,
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture.ToString(),
            findings);
    }

    private static void CheckSystem(List<DiagnosticFinding> findings)
        => findings.Add(new(
            "System",
            "Operating system",
            OperatingSystem.IsWindows() ? DiagnosticStatus.Passed : DiagnosticStatus.Warning,
            RuntimeInformation.OSDescription,
            OperatingSystem.IsWindows() ? null : "These reference templates target Windows."));

    private void CheckDirectories(List<DiagnosticFinding> findings)
    {
        foreach (var name in new[] { "Config", "Data", "Backups", "Logs", "Reports", "Temp" })
        {
            var path = Path.Combine(_options.DataRoot, name);
            if (!Directory.Exists(path))
            {
                findings.Add(new("Storage", name, DiagnosticStatus.Failed,
                    $"Missing directory: {path}", "Run Product.Repair --repair-folders."));
                continue;
            }

            var probe = Path.Combine(path, $".write-{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(probe, "test");
                File.Delete(probe);
                findings.Add(new("Storage", name, DiagnosticStatus.Passed,
                    $"Directory is writable: {path}"));
            }
            catch (Exception ex)
            {
                findings.Add(new("Storage", name, DiagnosticStatus.Failed,
                    $"Directory is not writable: {path}. {ex.Message}",
                    "Review NTFS permissions."));
            }
        }
    }

    private void CheckConfiguration(List<DiagnosticFinding> findings)
    {
        if (!File.Exists(_options.RuntimeConfigurationPath))
        {
            findings.Add(new("Configuration", "Runtime JSON", DiagnosticStatus.Failed,
                $"Missing: {_options.RuntimeConfigurationPath}",
                "Run ConfigTool or the first-run wizard."));
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(_options.RuntimeConfigurationPath));
            var validConnection = doc.RootElement.TryGetProperty("ConnectionStrings", out var c)
                && c.TryGetProperty("DefaultConnection", out var d)
                && d.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(d.GetString());

            findings.Add(new("Configuration", "Runtime JSON",
                validConnection ? DiagnosticStatus.Passed : DiagnosticStatus.Warning,
                validConnection ? "Configuration is valid." : "Configuration is valid, but the default connection is empty."));
        }
        catch (Exception ex)
        {
            findings.Add(new("Configuration", "Runtime JSON", DiagnosticStatus.Failed,
                $"Invalid JSON: {ex.Message}", "Restore the backup or run ConfigTool."));
        }
    }

    private void CheckPort(List<DiagnosticFinding> findings)
    {
        try
        {
            var listening = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(endpoint => endpoint.Port == _options.Port);

            findings.Add(new("Network", "Application port",
                listening ? DiagnosticStatus.Passed : DiagnosticStatus.Warning,
                listening ? $"Port {_options.Port} is listening." : $"Port {_options.Port} is not listening."));
        }
        catch (Exception ex)
        {
            findings.Add(new("Network", "Application port", DiagnosticStatus.Warning,
                $"Could not inspect listeners: {ex.Message}"));
        }
    }

    private async Task CheckHealthAsync(List<DiagnosticFinding> findings, CancellationToken token)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
        try
        {
            using var response = await client.GetAsync(
                $"http://127.0.0.1:{_options.Port}{_options.HealthPath}",
                token);

            findings.Add(new("Application", "Health endpoint",
                response.IsSuccessStatusCode ? DiagnosticStatus.Passed : DiagnosticStatus.Failed,
                $"HTTP {(int)response.StatusCode}."));
        }
        catch (Exception ex)
        {
            findings.Add(new("Application", "Health endpoint", DiagnosticStatus.Warning,
                $"Unavailable: {ex.Message}", "Start the application and verify the health path."));
        }
    }

    private async Task CheckDatabaseAsync(List<DiagnosticFinding> findings, CancellationToken token)
    {
        if (!File.Exists(_options.RuntimeConfigurationPath))
        {
            findings.Add(new("Database", "Connection", DiagnosticStatus.Skipped,
                "Skipped because runtime configuration is missing."));
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(_options.RuntimeConfigurationPath));
            var value = doc.RootElement.GetProperty("ConnectionStrings")
                .GetProperty("DefaultConnection").GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                findings.Add(new("Database", "Connection", DiagnosticStatus.Skipped,
                    "Skipped because the connection string is empty."));
                return;
            }

            var builder = new SqlConnectionStringBuilder(value)
            {
                ConnectTimeout = Math.Min(new SqlConnectionStringBuilder(value).ConnectTimeout, 8)
            };

            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(token);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT DB_NAME();";
            command.CommandTimeout = 8;
            var database = (string?)await command.ExecuteScalarAsync(token);

            findings.Add(new("Database", "Connection", DiagnosticStatus.Passed,
                $"Connected to '{database ?? "(unknown)"}'."));
        }
        catch (Exception ex)
        {
            findings.Add(new("Database", "Connection", DiagnosticStatus.Failed,
                $"Connection failed: {Sanitize(ex.Message)}",
                "Run DbConnectionTester."));
        }
    }

    private void CheckDisk(List<DiagnosticFinding> findings)
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(_options.DataRoot))!);
            var freeGiB = drive.AvailableFreeSpace / 1024d / 1024d / 1024d;

            findings.Add(new("Storage", "Free disk space",
                freeGiB >= 2 ? DiagnosticStatus.Passed : DiagnosticStatus.Warning,
                $"{freeGiB:F2} GiB available.",
                freeGiB >= 2 ? null : "Free space before backup, migration, or update."));
        }
        catch (Exception ex)
        {
            findings.Add(new("Storage", "Free disk space", DiagnosticStatus.Warning,
                $"Could not determine free space: {ex.Message}"));
        }
    }

    private void CheckLogs(List<DiagnosticFinding> findings)
    {
        var path = Path.Combine(_options.DataRoot, "Logs");
        var count = Directory.Exists(path)
            ? Directory.GetFiles(path, "*.log").Length
            : 0;

        findings.Add(new("Logging", "Log files",
            count > 0 ? DiagnosticStatus.Passed : DiagnosticStatus.Warning,
            count > 0 ? $"{count} log file(s) found." : "No log files found."));
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
