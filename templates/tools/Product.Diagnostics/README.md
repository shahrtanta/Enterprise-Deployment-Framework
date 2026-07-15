# Product.Diagnostics

Runs deployment and runtime checks and can generate a sanitized support ZIP.

```powershell
Product.Diagnostics.exe `
  --configuration "C:\ProgramData\Company\Product\Config\appsettings.runtime.json" `
  --support-bundle `
  --output "$env:USERPROFILE\Desktop\Product Support"
```

Exit codes: `0` healthy/warnings only, `2` failed checks, `3` engine failure.
