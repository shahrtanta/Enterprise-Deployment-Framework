[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PublishDirectory,

    [int]$Port = 5080
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$serverExe = Join-Path $PublishDirectory "Product.Reference.Web.exe"
if (-not (Test-Path $serverExe)) {
    throw "Server executable not found: $serverExe"
}

$required = @(
    "Product.Reference.Web.exe",
    "Product.Reference.Web.dll",
    "Product.AppLauncher.exe",
    "appsettings.json",
    "appsettings.Production.json",
    "wwwroot\css\site.css",
    "wwwroot\js\site.js"
)

foreach ($item in $required) {
    if (-not (Test-Path (Join-Path $PublishDirectory $item))) {
        throw "Missing publish artifact: $item"
    }
}

$process = Start-Process `
    -FilePath $serverExe `
    -WorkingDirectory $PublishDirectory `
    -WindowStyle Hidden `
    -PassThru `
    -Environment @{
        ASPNETCORE_ENVIRONMENT = "Production"
        ASPNETCORE_URLS = "http://127.0.0.1:$Port"
    }

try {
    $deadline = (Get-Date).AddSeconds(45)
    $healthy = $false

    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest `
                -Uri "http://127.0.0.1:$Port/health" `
                -UseBasicParsing `
                -TimeoutSec 3

            if ($response.StatusCode -eq 200) {
                $healthy = $true
                break
            }
        } catch {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $healthy) {
        throw "Health endpoint did not become ready."
    }

    $setup = Invoke-WebRequest `
        -Uri "http://127.0.0.1:$Port/Setup" `
        -UseBasicParsing `
        -TimeoutSec 5

    if ($setup.StatusCode -ne 200) {
        throw "First-run setup page did not return HTTP 200."
    }

    if ($setup.Content -notmatch "Configure the database") {
        throw "First-run setup content was not detected."
    }

    Write-Host "Smoke test passed."
}
finally {
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
