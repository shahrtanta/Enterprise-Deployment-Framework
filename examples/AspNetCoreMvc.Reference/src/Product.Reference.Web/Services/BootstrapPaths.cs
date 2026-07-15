namespace Product.Reference.Web.Services;

public sealed record BootstrapPaths(
    string CompanyName,
    string ApplicationName,
    string DataRoot,
    string ConfigDirectory,
    string DataDirectory,
    string DatabaseDirectory,
    string BackupDirectory,
    string LogsDirectory,
    string ReportsDirectory,
    string TempDirectory,
    string UploadsDirectory,
    string RuntimeConfigurationPath,
    string SetupCompleteMarker)
{
    public static BootstrapPaths Create(
        string companyName,
        string applicationName)
    {
        var dataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            companyName,
            applicationName);

        return new BootstrapPaths(
            companyName,
            applicationName,
            dataRoot,
            Path.Combine(dataRoot, "Config"),
            Path.Combine(dataRoot, "Data"),
            Path.Combine(dataRoot, "Database"),
            Path.Combine(dataRoot, "Backups"),
            Path.Combine(dataRoot, "Logs"),
            Path.Combine(dataRoot, "Reports"),
            Path.Combine(dataRoot, "Temp"),
            Path.Combine(dataRoot, "Uploads"),
            Path.Combine(dataRoot, "Config", "appsettings.runtime.json"),
            Path.Combine(dataRoot, "Config", "setup.complete"));
    }
}
