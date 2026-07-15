using System.Text.Json;

namespace Product.AppLauncher;

internal sealed class LauncherOptions
{
    public string ApplicationName { get; init; } = "Product";
    public string CompanyName { get; init; } = "Company";
    public string ServerExecutable { get; init; } = "Product.Web.exe";
    public string ServerDll { get; init; } = "Product.Web.dll";
    public int Port { get; init; } = 5080;
    public string HealthPath { get; init; } = "/health";
    public string BrowserPath { get; init; } = "/";
    public int StartupTimeoutSeconds { get; init; } = 60;

    public static LauncherOptions Load(string baseDirectory)
    {
        var settingsPath = Path.Combine(baseDirectory, "appsettings.json");
        if (!File.Exists(settingsPath))
        {
            return new LauncherOptions();
        }

        using var stream = File.OpenRead(settingsPath);
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;

        string ReadString(string section, string name, string fallback)
        {
            if (root.TryGetProperty(section, out var sectionElement) &&
                sectionElement.TryGetProperty(name, out var value) &&
                value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? fallback;
            }

            return fallback;
        }

        int ReadInt(string section, string name, int fallback)
        {
            if (root.TryGetProperty(section, out var sectionElement) &&
                sectionElement.TryGetProperty(name, out var value) &&
                value.TryGetInt32(out var parsed))
            {
                return parsed;
            }

            return fallback;
        }

        return new LauncherOptions
        {
            ApplicationName = ReadString("ApplicationSettings", "ApplicationName", "Product"),
            CompanyName = ReadString("ApplicationSettings", "CompanyName", "Company"),
            ServerExecutable = ReadString("Launcher", "ServerExecutable", "Product.Web.exe"),
            ServerDll = ReadString("Launcher", "ServerDll", "Product.Web.dll"),
            Port = ReadInt("ApplicationSettings", "Port", 5080),
            HealthPath = ReadString("Launcher", "HealthPath", "/health"),
            BrowserPath = ReadString("Launcher", "BrowserPath", "/"),
            StartupTimeoutSeconds = ReadInt("Launcher", "StartupTimeoutSeconds", 60)
        };
    }
}
