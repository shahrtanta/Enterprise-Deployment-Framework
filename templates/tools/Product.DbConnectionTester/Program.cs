using System.Text.Json;

namespace Product.DbConnectionTester;

internal static class Program
{
    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var requestPath = GetArgument(args, "--request-ini");
            var resultPath = GetArgument(args, "--result-json");
            if (string.IsNullOrWhiteSpace(requestPath))
            {
                MessageBox.Show("Use --request-ini <path> [--result-json <path>] [--silent].", "Database Connection Tester");
                return 1;
            }
            var result = await SqlConnectionTester.TestAsync(RequestMapper.FromIni(requestPath));
            if (!string.IsNullOrWhiteSpace(resultPath))
                await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            if (!args.Contains("--silent", StringComparer.OrdinalIgnoreCase))
                MessageBox.Show(result.Message, "Database Connection Test", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            return result.Success ? 0 : 2;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Database Connection Tester", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 3;
        }
    }

    private static string? GetArgument(string[] args, string name)
    {
        var index = Array.FindIndex(args, value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
