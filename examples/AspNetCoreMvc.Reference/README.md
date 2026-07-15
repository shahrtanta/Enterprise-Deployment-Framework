# ASP.NET Core MVC Reference Application

This example demonstrates how EDF components work together.

## Included

- ASP.NET Core MVC on .NET 8.
- Loopback-only Kestrel endpoint.
- Health endpoint.
- ProgramData storage.
- Runtime JSON loaded outside Program Files.
- First-run middleware.
- Database selection wizard.
- Test Connection endpoint.
- Atomic runtime configuration.
- Setup completion marker.
- Offline CSS and JavaScript.
- Publish and smoke-test scripts.

## Run

```powershell
dotnet run --project .\src\Product.Reference.Web\Product.Reference.Web.csproj
```

Open:

```text
http://127.0.0.1:5080/Setup
```

## Publish

```powershell
.\scripts\publish-reference.ps1
```

## Smoke test

```powershell
.\scripts\smoke-test.ps1 `
  -PublishDirectory ".\artifacts\publish"
```

The reference uses SQL Server/LocalDB. Adapt the connection builder for SQLite or another provider.
