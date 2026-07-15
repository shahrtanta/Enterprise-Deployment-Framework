[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PublishDirectory,

    [string[]]$RequiredFiles = @(
        "appsettings.json",
        "Product.AppLauncher.exe"
    )
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (-not (Test-Path $PublishDirectory -PathType Container)) {
    throw "Publish directory does not exist: $PublishDirectory"
}

$errors = [System.Collections.Generic.List[string]]::new()

foreach ($file in $RequiredFiles) {
    if (-not (Test-Path (Join-Path $PublishDirectory $file) -PathType Leaf)) {
        $errors.Add("Missing required file: $file")
    }
}

$developmentSettings = Join-Path $PublishDirectory "appsettings.Development.json"
if (Test-Path $developmentSettings) {
    $errors.Add("Development settings must not be included.")
}

$remotePatterns = @(
    "https://fonts.googleapis.com",
    "https://fonts.gstatic.com",
    "cdnjs.cloudflare.com",
    "cdn.jsdelivr.net",
    "unpkg.com"
)

$webFiles = Get-ChildItem $PublishDirectory -Recurse -File |
    Where-Object { $_.Extension -in ".html", ".htm", ".cshtml", ".css", ".js" }

foreach ($webFile in $webFiles) {
    $content = Get-Content $webFile.FullName -Raw -ErrorAction SilentlyContinue
    foreach ($pattern in $remotePatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            $errors.Add("Remote dependency '$pattern' found in $($webFile.FullName)")
        }
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host "Publish artifact validation passed."
