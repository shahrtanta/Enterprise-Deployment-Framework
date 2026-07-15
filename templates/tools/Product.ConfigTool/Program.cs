using Product.Deployment.Core;

namespace Product.ConfigTool;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var requestPath = GetArgument(args, "--apply-installer-ini");
            if (string.IsNullOrWhiteSpace(requestPath))
            {
                MessageBox.Show("Use --apply-installer-ini <path> [--silent].", "Configuration Tool");
                return 1;
            }
            var (targetPath, configuration) = InstallerConfigurationMapper.FromIni(requestPath);
            AtomicJsonConfigurationWriter.Apply(targetPath, configuration);
            if (!args.Contains("--silent", StringComparer.OrdinalIgnoreCase))
                MessageBox.Show("Configuration was saved successfully.", configuration.ApplicationName);
            return 0;
        }
        catch (Exception ex)
        {
            if (!args.Contains("--silent", StringComparer.OrdinalIgnoreCase))
                MessageBox.Show(ex.Message, "Configuration Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 2;
        }
    }

    private static string? GetArgument(string[] args, string name)
    {
        var index = Array.FindIndex(args, value => string.Equals(value, name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
