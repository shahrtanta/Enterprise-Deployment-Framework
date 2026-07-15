namespace Product.Reference.Web.Services;

public static class DirectoryBootstrap
{
    public static void EnsureCreated(BootstrapPaths paths)
    {
        foreach (var path in new[]
                 {
                     paths.DataRoot,
                     paths.ConfigDirectory,
                     paths.DataDirectory,
                     paths.DatabaseDirectory,
                     paths.BackupDirectory,
                     paths.LogsDirectory,
                     paths.ReportsDirectory,
                     paths.TempDirectory,
                     paths.UploadsDirectory
                 })
        {
            Directory.CreateDirectory(path);
        }
    }
}
