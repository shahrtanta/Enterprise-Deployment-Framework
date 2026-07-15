using System.Text.Json;
using System.Text.Json.Nodes;
using Product.Reference.Web.Models;

namespace Product.Reference.Web.Services;

public sealed class RuntimeConfigurationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BootstrapPaths _paths;

    public RuntimeConfigurationService(BootstrapPaths paths)
    {
        _paths = paths;
    }

    public async Task SaveAsync(
        SetupDatabaseModel model,
        int port,
        CancellationToken cancellationToken = default)
    {
        var connectionString = ConnectionStringBuilderService.Build(model, _paths);

        var root = new JsonObject
        {
            ["ApplicationSettings"] = new JsonObject
            {
                ["CompanyName"] = _paths.CompanyName,
                ["ApplicationName"] = _paths.ApplicationName,
                ["Port"] = port,
                ["RequiresFirstRun"] = false,
                ["DataRoot"] = _paths.DataRoot,
                ["RuntimeConfigurationPath"] = _paths.RuntimeConfigurationPath
            },
            ["DatabaseSettings"] = new JsonObject
            {
                ["DatabaseType"] = model.DatabaseType,
                ["LocalProvider"] = model.LocalProvider,
                ["ServerName"] = string.Equals(
                    model.DatabaseType,
                    "Local",
                    StringComparison.OrdinalIgnoreCase)
                        ? string.Empty
                        : model.ServerName,
                ["DatabaseName"] = model.DatabaseName,
                ["AuthenticationType"] = model.AuthenticationType,
                ["DataPath"] = _paths.DatabaseDirectory,
                ["AutoDetectSql"] = true,
                ["AutoDetectService"] = true
            },
            ["ConnectionStrings"] = new JsonObject
            {
                ["DefaultConnection"] = connectionString
            },
            ["Update"] = new JsonObject
            {
                ["Enabled"] = false,
                ["ManifestUrl"] = string.Empty
            }
        };

        Directory.CreateDirectory(_paths.ConfigDirectory);

        var target = _paths.RuntimeConfigurationPath;
        var backup = target + ".bak";
        var temp = target + ".tmp." + Guid.NewGuid().ToString("N");

        if (File.Exists(target))
        {
            File.Copy(target, backup, true);
        }

        try
        {
            await File.WriteAllTextAsync(
                temp,
                root.ToJsonString(JsonOptions),
                cancellationToken);

            if (File.Exists(target))
            {
                File.Replace(temp, target, backup, true);
            }
            else
            {
                File.Move(temp, target);
            }
        }
        finally
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
        }
    }

    public void MarkSetupComplete()
    {
        File.WriteAllText(
            _paths.SetupCompleteMarker,
            DateTimeOffset.UtcNow.ToString("O"));
    }

    public bool IsSetupComplete()
        => File.Exists(_paths.SetupCompleteMarker);
}
