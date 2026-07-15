using System.Text.Json.Serialization;

namespace Product.Update;

internal sealed record UpdateManifest
{
    public string Product { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string PackageFile { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string? MinimumVersion { get; init; }
    public string? ReleaseNotes { get; init; }
    public IReadOnlyList<string> PreUpdateCommands { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PostUpdateCommands { get; init; } = Array.Empty<string>();
}
