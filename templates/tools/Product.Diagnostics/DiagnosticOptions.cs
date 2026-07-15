using System.Text.Json;

namespace Product.Diagnostics;

internal sealed record DiagnosticOptions
{
    public string CompanyName { get; init; } = "Company";
    public string ApplicationName { get; init; } = "Product";
    public string ApplicationVersion { get; init; } = "unknown";
    public int Port { get; init; } = 5080;
    public string HealthPath { get; init; } = "/health";
    public string DataRoot { get; init; } = string.Empty;
    public string RuntimeConfigurationPath { get; init; } = string.Empty;

    public static DiagnosticOptions Load(string baseDirectory, string? explicitPath)
    {
        var path = explicitPath is not null
            ? Path.GetFullPath(explicitPath)
            : Path.Combine(baseDirectory, "appsettings.json");

        if (!File.Exists(path))
            return Defaults("Company", "Product");

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        string ReadString(string section, string name, string fallback)
            => root.TryGetProperty(section, out var s)
               && s.TryGetProperty(name, out var v)
               && v.ValueKind == JsonValueKind.String
                ? v.GetString() ?? fallback
                : fallback;

        int ReadInt(string section, string name, int fallback)
            => root.TryGetProperty(section, out var s)
               && s.TryGetProperty(name, out var v)
               && v.TryGetInt32(out var parsed)
                ? parsed
                : fallback;

        var company = ReadString("ApplicationSettings", "CompanyName", "Company");
        var app = ReadString("ApplicationSettings", "ApplicationName", "Product");
        var defaults = Defaults(company, app);

        return defaults with
        {
            ApplicationVersion = ReadString("ApplicationSettings", "Version", "unknown"),
            Port = ReadInt("ApplicationSettings", "Port", 5080),
            HealthPath = ReadString("Launcher", "HealthPath", "/health"),
            DataRoot = ReadString("ApplicationSettings", "DataRoot", defaults.DataRoot),
            RuntimeConfigurationPath = ReadString(
                "ApplicationSettings",
                "RuntimeConfigurationPath",
                defaults.RuntimeConfigurationPath)
        };
    }

    private static DiagnosticOptions Defaults(string company, string app)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            company,
            app);

        return new DiagnosticOptions
        {
            CompanyName = company,
            ApplicationName = app,
            DataRoot = root,
            RuntimeConfigurationPath = Path.Combine(root, "Config", "appsettings.runtime.json")
        };
    }
}
