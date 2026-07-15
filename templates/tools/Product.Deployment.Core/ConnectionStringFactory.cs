using System.Data.Common;

namespace Product.Deployment.Core;

public static class ConnectionStringFactory
{
    public static string Build(DeploymentConfiguration configuration) => Build(new ConnectionTestRequest
    {
        DatabaseType = configuration.DatabaseType,
        LocalProvider = configuration.LocalProvider,
        DataPath = configuration.DataPath,
        ServerName = configuration.ServerName,
        DatabaseName = configuration.DatabaseName,
        AuthenticationType = configuration.AuthenticationType,
        UserName = configuration.UserName,
        Password = configuration.Password,
        TrustServerCertificate = configuration.TrustServerCertificate
    });

    public static string Build(ConnectionTestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.DatabaseType == DatabaseType.Local)
        {
            if (!string.Equals(request.LocalProvider, "LocalDB", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("The reference implementation currently supports LocalDB. Add a provider-specific SQLite adapter when required.");

            var dataPath = Path.GetFullPath(request.DataPath);
            Directory.CreateDirectory(dataPath);
            var mdfPath = Path.Combine(dataPath, request.DatabaseName + ".mdf");
            var builder = new DbConnectionStringBuilder
            {
                ["Data Source"] = @"(LocalDB)\MSSQLLocalDB",
                ["AttachDbFilename"] = mdfPath,
                ["Initial Catalog"] = request.DatabaseName,
                ["Integrated Security"] = true,
                ["Connect Timeout"] = request.TimeoutSeconds,
                ["TrustServerCertificate"] = request.TrustServerCertificate,
                ["MultipleActiveResultSets"] = true
            };
            return builder.ConnectionString;
        }

        if (string.IsNullOrWhiteSpace(request.ServerName)) throw new ArgumentException("Server name is required.");
        if (string.IsNullOrWhiteSpace(request.DatabaseName)) throw new ArgumentException("Database name is required.");

        var sqlBuilder = new DbConnectionStringBuilder
        {
            ["Data Source"] = request.ServerName.Trim(),
            ["Initial Catalog"] = request.DatabaseName.Trim(),
            ["Connect Timeout"] = request.TimeoutSeconds,
            ["TrustServerCertificate"] = request.TrustServerCertificate,
            ["Encrypt"] = true,
            ["MultipleActiveResultSets"] = true,
            ["Application Name"] = "Product Deployment Tools"
        };

        if (request.AuthenticationType == AuthenticationType.Windows)
            sqlBuilder["Integrated Security"] = true;
        else
        {
            if (string.IsNullOrWhiteSpace(request.UserName)) throw new ArgumentException("SQL Server user name is required.");
            sqlBuilder["User ID"] = request.UserName;
            sqlBuilder["Password"] = request.Password;
            sqlBuilder["Integrated Security"] = false;
        }
        return sqlBuilder.ConnectionString;
    }

    public static string Redact(string connectionString)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        foreach (var key in new[] { "Password", "Pwd" })
            if (builder.ContainsKey(key)) builder[key] = "***";
        return builder.ConnectionString;
    }
}
