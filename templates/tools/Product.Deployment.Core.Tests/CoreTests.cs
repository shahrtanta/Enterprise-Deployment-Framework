using System.Text.Json;
using Product.Deployment.Core;

namespace Product.Deployment.Core.Tests;

public sealed class CoreTests
{
    [Fact]
    public void Redact_Removes_Password()
    {
        var value = ConnectionStringFactory.Redact(
            "Data Source=server;Initial Catalog=db;User ID=user;Password=secret;");

        Assert.DoesNotContain("secret", value, StringComparison.Ordinal);
        Assert.Contains("***", value, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_Preserves_Unrelated_Json()
    {
        var directory = Path.Combine(Path.GetTempPath(), "edf", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "runtime.json");

        try
        {
            File.WriteAllText(path, """{"Unrelated":{"KeepMe":true}}""");

            AtomicJsonConfigurationWriter.Apply(path, new DeploymentConfiguration
            {
                DataPath = Path.Combine(directory, "Data"),
                DatabaseName = "TestDb"
            });

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            Assert.True(document.RootElement.GetProperty("Unrelated").GetProperty("KeepMe").GetBoolean());
            Assert.True(File.Exists(path + ".bak"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
