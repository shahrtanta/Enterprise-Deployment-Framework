# Product.Repair

Conservative, data-preserving repair utility.

```powershell
Product.Repair.exe `
  --company Company `
  --application Product `
  --repair-folders `
  --repair-configuration `
  --clear-temp
```

Use `--dry-run` to preview changes. The tool never deletes databases, backups, uploads, or valid configuration.
