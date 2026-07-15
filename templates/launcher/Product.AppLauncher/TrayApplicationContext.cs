namespace Product.AppLauncher;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly ServerManager _serverManager;
    private readonly LauncherOptions _options;
    private readonly LauncherLogger _logger;
    private readonly NotifyIcon _notifyIcon;

    public TrayApplicationContext(
        ServerManager serverManager,
        LauncherOptions options,
        LauncherLogger logger)
    {
        _serverManager = serverManager;
        _options = options;
        _logger = logger;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Application", null, (_, _) => _serverManager.OpenBrowser());
        menu.Items.Add("Server Status", null, async (_, _) => await ShowStatusAsync());
        menu.Items.Add("Open Logs", null, (_, _) => OpenLogs());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit and Stop Safely", null, async (_, _) => await ExitAsync());

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = _options.ApplicationName,
            Visible = true,
            ContextMenuStrip = menu
        };

        _notifyIcon.DoubleClick += (_, _) => _serverManager.OpenBrowser();
    }

    private async Task ShowStatusAsync()
    {
        var ready = await _serverManager.IsApplicationReadyAsync();
        MessageBox.Show(
            ready ? "The application server is running." : "The application server is not ready.",
            _options.ApplicationName,
            MessageBoxButtons.OK,
            ready ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void OpenLogs()
    {
        try
        {
            var logs = _logger.DirectoryPath;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = logs,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.Write("Unable to open logs directory.", ex);
        }
    }


    private async Task ExitAsync()
    {
        _notifyIcon.Visible = false;
        try
        {
            await _serverManager.StopAsync();
        }
        catch (Exception ex)
        {
            _logger.Write("Shutdown failed.", ex);
        }

        _notifyIcon.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
