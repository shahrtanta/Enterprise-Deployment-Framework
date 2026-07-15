using Product.Deployment.Core;

namespace Product.ConfigTool;

internal static class InstallerConfigurationMapper
{
    public static (string TargetPath, DeploymentConfiguration Configuration) FromIni(string path)
    {
        var values = InstallerIni.Read(path);
        var configuration = new DeploymentConfiguration
        {
            CompanyName = values.Optional("CompanyName", "Company"),
            ApplicationName = values.Optional("ApplicationName", "Product"),
            Port = values.Integer("Port", 5080),
            DatabaseType = Enum.TryParse<DatabaseType>(values.Optional("DatabaseType", "Local"), true, out var dbType) ? dbType : DatabaseType.Local,
            LocalProvider = values.Optional("LocalProvider", "LocalDB"),
            DataPath = values.Required("DataPath"),
            ServerName = values.Optional("ServerName", @".\SQLEXPRESS"),
            DatabaseName = values.Optional("DatabaseName", "ProductDB"),
            AuthenticationType = Enum.TryParse<AuthenticationType>(values.Optional("AuthenticationType", "Windows"), true, out var auth) ? auth : AuthenticationType.Windows,
            UserName = values.Optional("UserName"),
            Password = values.Optional("Password"),
            TrustServerCertificate = values.Boolean("TrustServerCertificate", true)
        };
        return (values.Required("TargetPath"), configuration);
    }
}
