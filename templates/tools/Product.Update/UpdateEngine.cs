using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace Product.Update;

internal sealed class UpdateEngine
{
    private readonly string _installDirectory;
    private readonly string _workingDirectory;

    public UpdateEngine(string installDirectory, string workingDirectory)
    {
        _installDirectory = Path.GetFullPath(installDirectory);
        _workingDirectory = Path.GetFullPath(workingDirectory);
    }

    public async Task ApplyAsync(
        string manifestPath,
        bool confirm,
        CancellationToken cancellationToken = default)
    {
        if (!confirm)
        {
            throw new InvalidOperationException(
                "Update requires the explicit --confirm-apply option.");
        }

        var manifest = JsonSerializer.Deserialize<UpdateManifest>(
            await File.ReadAllTextAsync(manifestPath, cancellationToken))
            ?? throw new InvalidDataException("Update manifest is invalid.");

        var packagePath = Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(manifestPath))!,
            manifest.PackageFile);

        if (!File.Exists(packagePath))
            throw new FileNotFoundException("Update package was not found.", packagePath);

        var actualHash = await ComputeSha256Async(packagePath, cancellationToken);
        if (!string.Equals(actualHash, manifest.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Update package hash does not match the manifest.");
        }

        Directory.CreateDirectory(_workingDirectory);

        var rollbackDirectory = Path.Combine(
            _workingDirectory,
            $"rollback-{DateTime.UtcNow:yyyyMMdd-HHmmss}");

        var stagingDirectory = Path.Combine(
            _workingDirectory,
            $"staging-{Guid.NewGuid():N}");

        Directory.CreateDirectory(rollbackDirectory);
        Directory.CreateDirectory(stagingDirectory);

        try
        {
            CopyDirectory(_installDirectory, rollbackDirectory);
            ZipFile.ExtractToDirectory(packagePath, stagingDirectory);

            ValidateStagedPackage(stagingDirectory);
            CopyDirectory(stagingDirectory, _installDirectory, overwrite: true);

            await File.WriteAllTextAsync(
                Path.Combine(_workingDirectory, "last-successful-update.txt"),
                $"{manifest.Product}|{manifest.Version}|{DateTime.UtcNow:O}",
                cancellationToken);
        }
        catch
        {
            CopyDirectory(rollbackDirectory, _installDirectory, overwrite: true);
            throw;
        }
        finally
        {
            try { Directory.Delete(stagingDirectory, true); } catch { }
        }
    }

    private static void ValidateStagedPackage(string stagingDirectory)
    {
        var manifest = Path.Combine(stagingDirectory, "deployment-manifest.json");
        if (!File.Exists(manifest))
        {
            throw new InvalidDataException(
                "The staged update does not contain deployment-manifest.json.");
        }
    }

    private static async Task<string> ComputeSha256Async(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static void CopyDirectory(
        string source,
        string destination,
        bool overwrite = false)
    {
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(
                file,
                Path.Combine(destination, Path.GetFileName(file)),
                overwrite);
        }

        foreach (var directory in Directory.GetDirectories(source))
        {
            var name = Path.GetFileName(directory);

            // Never package or restore mutable customer data from install directory.
            if (string.Equals(name, "Data", StringComparison.OrdinalIgnoreCase))
                continue;

            CopyDirectory(
                directory,
                Path.Combine(destination, name),
                overwrite);
        }
    }
}
