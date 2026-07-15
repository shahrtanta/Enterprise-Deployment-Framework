using System.Text.Json;
using System.Text.Json.Nodes;

namespace Product.Deployment.Core;

public static class AtomicJsonConfigurationWriter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Apply(string targetPath, DeploymentConfiguration configuration)
    {
        var fullPath = Path.GetFullPath(targetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        JsonObject root = File.Exists(fullPath)
            ? JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject ?? throw new InvalidDataException("Configuration must be a JSON object.")
            : new JsonObject();

        root["ApplicationSettings"] = Merge(root["ApplicationSettings"] as JsonObject, new JsonObject
        {
            ["CompanyName"] = configuration.CompanyName,
            ["ApplicationName"] = configuration.ApplicationName,
            ["Port"] = configuration.Port,
            ["RequiresFirstRun"] = false
        });
        root["DatabaseSettings"] = Merge(root["DatabaseSettings"] as JsonObject, new JsonObject
        {
            ["DatabaseType"] = configuration.DatabaseType == DatabaseType.Local ? "Local" : "InternalServer",
            ["LocalProvider"] = configuration.LocalProvider,
            ["ServerName"] = configuration.DatabaseType == DatabaseType.Local ? string.Empty : configuration.ServerName,
            ["DatabaseName"] = configuration.DatabaseName,
            ["AuthenticationType"] = configuration.AuthenticationType == AuthenticationType.Windows ? "Windows" : "SqlServer",
            ["DataPath"] = configuration.DataPath,
            ["AutoDetectSql"] = true,
            ["AutoDetectService"] = true
        });
        root["ConnectionStrings"] = Merge(root["ConnectionStrings"] as JsonObject, new JsonObject
        {
            ["DefaultConnection"] = ConnectionStringFactory.Build(configuration)
        });

        var backupPath = fullPath + ".bak";
        if (File.Exists(fullPath)) File.Copy(fullPath, backupPath, true);
        var tempPath = fullPath + ".tmp." + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(tempPath, root.ToJsonString(Options));
            if (File.Exists(fullPath)) File.Replace(tempPath, fullPath, backupPath, true);
            else File.Move(tempPath, fullPath);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    private static JsonObject Merge(JsonObject? existing, JsonObject updates)
    {
        var result = existing?.DeepClone() as JsonObject ?? new JsonObject();
        foreach (var pair in updates) result[pair.Key] = pair.Value?.DeepClone();
        return result;
    }
}
