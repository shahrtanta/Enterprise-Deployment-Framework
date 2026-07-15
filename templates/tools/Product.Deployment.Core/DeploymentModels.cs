namespace Product.Deployment.Core;

public enum DatabaseType { Local, InternalServer }
public enum AuthenticationType { Windows, SqlServer }

public sealed record DeploymentConfiguration
{
    public string CompanyName { get; init; } = "Company";
    public string ApplicationName { get; init; } = "Product";
    public int Port { get; init; } = 5080;
    public DatabaseType DatabaseType { get; init; } = DatabaseType.Local;
    public string LocalProvider { get; init; } = "LocalDB";
    public string DataPath { get; init; } = string.Empty;
    public string ServerName { get; init; } = @".\SQLEXPRESS";
    public string DatabaseName { get; init; } = "ProductDB";
    public AuthenticationType AuthenticationType { get; init; } = AuthenticationType.Windows;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool TrustServerCertificate { get; init; } = true;
}

public sealed record ConnectionTestRequest
{
    public DatabaseType DatabaseType { get; init; } = DatabaseType.Local;
    public string LocalProvider { get; init; } = "LocalDB";
    public string DataPath { get; init; } = string.Empty;
    public string ServerName { get; init; } = @".\SQLEXPRESS";
    public string DatabaseName { get; init; } = "ProductDB";
    public AuthenticationType AuthenticationType { get; init; } = AuthenticationType.Windows;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool TrustServerCertificate { get; init; } = true;
    public int TimeoutSeconds { get; init; } = 8;
}

public sealed record ConnectionTestResult(bool Success, string Message, string? ServerVersion = null, string? Database = null, string? ErrorCode = null);
