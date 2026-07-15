using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace Product.AppLauncher;

internal sealed class ServerManager : IDisposable
{
    private readonly LauncherOptions _options;
    private readonly LauncherPaths _paths;
    private readonly LauncherLogger _logger;
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
    private Process? _ownedProcess;

    public ServerManager(LauncherOptions options, LauncherPaths paths, LauncherLogger logger)
    {
        _options = options;
        _paths = paths;
        _logger = logger;
    }

    public Uri BaseUri => new($"http://127.0.0.1:{_options.Port}");
    public Uri HealthUri => new(BaseUri, _options.HealthPath);
    public Uri BrowserUri => new(BaseUri, _options.BrowserPath);

    public async Task<bool> IsApplicationReadyAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync(HealthUri);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public bool IsPortListening()
    {
        try
        {
            var listener = System.Net.NetworkInformation.IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners();

            return listener.Any(endpoint => endpoint.Port == _options.Port);
        }
        catch (Exception ex)
        {
            _logger.Write("Unable to inspect TCP listeners.", ex);
            return false;
        }
    }

    public async Task<StartResult> EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (await IsApplicationReadyAsync())
        {
            return StartResult.AlreadyRunning;
        }

        if (IsPortListening())
        {
            return StartResult.PortConflict;
        }

        var startInfo = BuildStartInfo();
        _logger.Write($"Starting server: {startInfo.FileName} {startInfo.Arguments}");

        _ownedProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The application process could not be started.");

        File.WriteAllText(_paths.PidFile, _ownedProcess.Id.ToString());

        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(_options.StartupTimeoutSeconds);
        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_ownedProcess.HasExited)
            {
                _logger.Write($"Server exited before becoming ready. Exit code: {_ownedProcess.ExitCode}");
                return StartResult.ProcessExited;
            }

            if (await IsApplicationReadyAsync())
            {
                _logger.Write("Server is ready.");
                return StartResult.Started;
            }

            await Task.Delay(500, cancellationToken);
        }

        _logger.Write("Server readiness timeout elapsed.");
        return StartResult.Timeout;
    }

    private ProcessStartInfo BuildStartInfo()
    {
        var executablePath = Path.Combine(_paths.BaseDirectory, _options.ServerExecutable);
        if (File.Exists(executablePath))
        {
            return CreateStartInfo(executablePath, string.Empty);
        }

        var dllPath = Path.Combine(_paths.BaseDirectory, _options.ServerDll);
        if (File.Exists(dllPath))
        {
            return CreateStartInfo("dotnet", Quote(dllPath));
        }

        throw new FileNotFoundException(
            $"Neither '{_options.ServerExecutable}' nor '{_options.ServerDll}' was found.",
            executablePath);
    }

    private ProcessStartInfo CreateStartInfo(string fileName, string arguments)
    {
        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = _paths.BaseDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            Environment =
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                ["ASPNETCORE_URLS"] = $"http://127.0.0.1:{_options.Port}"
            }
        };
    }

    public void OpenBrowser()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = BrowserUri.ToString(),
            UseShellExecute = true
        });
    }

    public async Task StopAsync()
    {
        var process = ResolveOwnedProcess();
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                _logger.Write($"Stopping server process {process.Id}.");
                process.CloseMainWindow();

                if (!await WaitForExitAsync(process, TimeSpan.FromSeconds(8)))
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Write("Failed to stop the server process.", ex);
            throw;
        }
        finally
        {
            TryDeletePidFile();
        }
    }

    private Process? ResolveOwnedProcess()
    {
        if (_ownedProcess is not null)
        {
            return _ownedProcess;
        }

        if (!File.Exists(_paths.PidFile) ||
            !int.TryParse(File.ReadAllText(_paths.PidFile), out var processId))
        {
            return null;
        }

        try
        {
            var process = Process.GetProcessById(processId);
            return process.StartTime <= DateTime.Now ? process : null;
        }
        catch
        {
            TryDeletePidFile();
            return null;
        }
    }

    private static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private void TryDeletePidFile()
    {
        try
        {
            if (File.Exists(_paths.PidFile))
            {
                File.Delete(_paths.PidFile);
            }
        }
        catch
        {
            // Best effort cleanup only.
        }
    }

    private static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    public void Dispose()
    {
        _ownedProcess?.Dispose();
        _httpClient.Dispose();
    }
}

internal enum StartResult
{
    Started,
    AlreadyRunning,
    PortConflict,
    ProcessExited,
    Timeout
}
