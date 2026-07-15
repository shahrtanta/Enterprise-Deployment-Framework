using Microsoft.Data.SqlClient;
using Product.Reference.Web.Models;

namespace Product.Reference.Web.Services;

public static class ConnectionStringBuilderService
{
    public static string Build(
        SetupDatabaseModel model,
        BootstrapPaths paths)
    {
        if (string.Equals(model.DatabaseType, "Local", StringComparison.OrdinalIgnoreCase))
        {
            var mdfPath = Path.Combine(
                paths.DatabaseDirectory,
                model.DatabaseName + ".mdf");

            return new SqlConnectionStringBuilder
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                AttachDBFilename = mdfPath,
                InitialCatalog = model.DatabaseName,
                IntegratedSecurity = true,
                TrustServerCertificate = model.TrustServerCertificate,
                Encrypt = true,
                ConnectTimeout = 8,
                MultipleActiveResultSets = true
            }.ConnectionString;
        }

        if (string.IsNullOrWhiteSpace(model.ServerName))
            throw new InvalidOperationException("Server name is required.");

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = model.ServerName.Trim(),
            InitialCatalog = model.DatabaseName.Trim(),
            TrustServerCertificate = model.TrustServerCertificate,
            Encrypt = true,
            ConnectTimeout = 8,
            MultipleActiveResultSets = true,
            ApplicationName = "Product Reference"
        };

        if (string.Equals(
                model.AuthenticationType,
                "SqlServer",
                StringComparison.OrdinalIgnoreCase))
        {
            builder.UserID = model.UserName;
            builder.Password = model.Password;
            builder.IntegratedSecurity = false;
        }
        else
        {
            builder.IntegratedSecurity = true;
        }

        return builder.ConnectionString;
    }
}
