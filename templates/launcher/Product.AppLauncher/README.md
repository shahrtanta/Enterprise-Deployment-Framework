# Product.AppLauncher

Reference Windows launcher for a locally hosted ASP.NET Core application.

## Features

- Single launcher instance.
- Self-contained WinForms executable.
- Reads port and process names from `appsettings.json`.
- Checks application health before launching duplicates.
- Detects unrelated port conflicts.
- Starts Kestrel without a console window.
- Opens the default browser after readiness.
- Provides a tray menu.
- Stores PID state and logs under ProgramData.
- Supports `--shutdown` for installer upgrade/uninstall workflows.

## Required application endpoint

Expose a local health endpoint matching `Launcher:HealthPath`, for example:

```csharp
app.MapHealthChecks("/health");
```

## Publish

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Replace `Product`, `Company`, executable names, and AppId before production use.
