[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$exampleRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Split-Path -Parent (Split-Path -Parent $exampleRoot)
$artifacts = Join-Path $exampleRoot "artifacts"
$publish = Join-Path $artifacts "publish"
$tools = Join-Path $artifacts "tools"

if (Test-Path $artifacts) {
    Remove-Item $artifacts -Recurse -Force
}

New-Item $publish -ItemType Directory -Force | Out-Null
New-Item $tools -ItemType Directory -Force | Out-Null

dotnet restore (Join-Path $exampleRoot "Product.Reference.slnx")
dotnet build (Join-Path $exampleRoot "Product.Reference.slnx") `
    -c $Configuration `
    --no-restore

dotnet publish `
    (Join-Path $exampleRoot "src\Product.Reference.Web\Product.Reference.Web.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    --no-build `
    -o $publish

$toolProjects = @(
    "Product.AppLauncher\Product.AppLauncher.csproj",
    "Product.ConfigTool\Product.ConfigTool.csproj",
    "Product.DbConnectionTester\Product.DbConnectionTester.csproj",
    "Product.Diagnostics\Product.Diagnostics.csproj",
    "Product.Repair\Product.Repair.csproj"
)

foreach ($toolProject in $toolProjects) {
    $isLauncher = $toolProject.StartsWith("Product.AppLauncher")
    $projectPath = if ($isLauncher) {
        Join-Path $repoRoot "templates\launcher\$toolProject"
    } else {
        Join-Path $repoRoot "templates\tools\$toolProject"
    }

    dotnet publish $projectPath `
        -c $Configuration `
        -r $Runtime `
        --self-contained true `
        -o $tools
}

Copy-Item (Join-Path $tools "Product.AppLauncher.exe") `
    (Join-Path $publish "Product.AppLauncher.exe") `
    -Force

$developmentSettings = Join-Path $publish "appsettings.Development.json"
if (Test-Path $developmentSettings) {
    Remove-Item $developmentSettings -Force
}

Write-Host "Reference publish completed: $publish"
