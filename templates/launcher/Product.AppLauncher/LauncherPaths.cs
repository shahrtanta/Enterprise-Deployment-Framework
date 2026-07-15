namespace Product.AppLauncher;

internal sealed class LauncherPaths
{
    public string BaseDirectory { get; }
    public string DataRoot { get; }
    public string LogsDirectory { get; }
    public string StateDirectory { get; }
    public string PidFile { get; }
    public string LauncherLog { get; }

    public LauncherPaths(LauncherOptions options)
    {
        BaseDirectory = AppContext.BaseDirectory;
        DataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            options.CompanyName,
            options.ApplicationName);

        LogsDirectory = Path.Combine(DataRoot, "Logs");
        StateDirectory = Path.Combine(DataRoot, "State");
        PidFile = Path.Combine(StateDirectory, "server.pid");
        LauncherLog = Path.Combine(LogsDirectory, "launcher.log");

        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(StateDirectory);
    }
}
