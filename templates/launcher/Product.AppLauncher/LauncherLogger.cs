namespace Product.AppLauncher;

internal sealed class LauncherLogger
{
    private readonly string _path;
    private readonly object _sync = new();

    public LauncherLogger(string path) => _path = path;

    public string DirectoryPath => Path.GetDirectoryName(_path) ?? AppContext.BaseDirectory;

    public void Write(string message, Exception? exception = null)
    {
        var line = $"{DateTimeOffset.Now:O} [{Environment.ProcessId}] {message}";
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        lock (_sync)
        {
            File.AppendAllText(_path, line + Environment.NewLine);
        }
    }
}
