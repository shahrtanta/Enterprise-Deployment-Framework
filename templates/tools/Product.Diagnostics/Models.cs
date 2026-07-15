namespace Product.Diagnostics;

internal enum DiagnosticStatus { Passed, Warning, Failed, Skipped }

internal sealed record DiagnosticFinding(
    string Category,
    string Check,
    DiagnosticStatus Status,
    string Message,
    string? Recommendation = null);

internal sealed record DiagnosticReport(
    string ApplicationName,
    string ApplicationVersion,
    DateTimeOffset GeneratedAtUtc,
    string MachineName,
    string OperatingSystem,
    string Architecture,
    IReadOnlyList<DiagnosticFinding> Findings);
