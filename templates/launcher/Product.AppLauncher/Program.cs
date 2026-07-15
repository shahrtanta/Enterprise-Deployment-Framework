using System.Threading;

namespace Product.AppLauncher;

internal static class Program
{
    private const string MutexName = @"Local\Product.AppLauncher.Singleton";

    [STAThread]
    private static async Task Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(initiallyOwned: true, MutexName, out var ownsMutex);
        if (!ownsMutex)
        {
            MessageBox.Show(
                "The application launcher is already running.",
                "Product",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var options = LauncherOptions.Load(AppContext.BaseDirectory);
        var paths = new LauncherPaths(options);
        var logger = new LauncherLogger(paths.LauncherLog);

        try
        {
            using var manager = new ServerManager(options, paths, logger);

            if (args.Any(arg => string.Equals(arg, "--shutdown", StringComparison.OrdinalIgnoreCase)))
            {
                await manager.StopAsync();
                return;
            }

            using var cts = new CancellationTokenSource();
            var result = await manager.EnsureStartedAsync(cts.Token);

            switch (result)
            {
                case StartResult.Started:
                case StartResult.AlreadyRunning:
                    manager.OpenBrowser();
                    Application.Run(new TrayApplicationContext(manager, options, logger));
                    break;

                case StartResult.PortConflict:
                    MessageBox.Show(
                        $"Port {options.Port} is already used by another process.",
                        options.ApplicationName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;

                case StartResult.ProcessExited:
                    MessageBox.Show(
                        "The application server exited before it became ready. Check the logs.",
                        options.ApplicationName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;

                case StartResult.Timeout:
                    MessageBox.Show(
                        "The application did not become ready before the startup timeout. Check the logs.",
                        options.ApplicationName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.Write("Fatal launcher error.", ex);
            MessageBox.Show(
                $"The application could not be started.{Environment.NewLine}{ex.Message}",
                options.ApplicationName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
