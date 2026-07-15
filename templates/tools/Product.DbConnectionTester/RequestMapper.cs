using Product.Deployment.Core;

namespace Product.DbConnectionTester;

internal static class RequestMapper
{
    public static ConnectionTestRequest FromIni(string path)
    {
        var values = InstallerIni.Read(path);
        return new ConnectionTestRequest
        {
            DatabaseType = Enum.TryParse<DatabaseType>(values.Optional("DatabaseType", "Local"), true, out var dbType) ? dbType : DatabaseType.Local,
            LocalProvider = values.Optional("LocalProvider", "LocalDB"),
            DataPath = values.Optional("DataPath", Path.Combine(Path.GetTempPath(), "Product", "Data")),
            ServerName = values.Optional("ServerName", @".\SQLEXPRESS"),
            DatabaseName = values.Optional("DatabaseName", "ProductDB"),
            AuthenticationType = Enum.TryParse<AuthenticationType>(values.Optional("AuthenticationType", "Windows"), true, out var auth) ? auth : AuthenticationType.Windows,
            UserName = values.Optional("UserName"),
            Password = values.Optional("Password"),
            TrustServerCertificate = values.Boolean("TrustServerCertificate", true),
            TimeoutSeconds = values.Integer("TimeoutSeconds", 8)
        };
    }
}
