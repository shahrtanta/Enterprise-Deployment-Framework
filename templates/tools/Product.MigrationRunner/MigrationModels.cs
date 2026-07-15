namespace Product.MigrationRunner;

internal sealed record MigrationScript(
    string Version,
    string Path,
    string Sha256);

internal sealed record MigrationResult(
    string Version,
    bool Success,
    string Message);
