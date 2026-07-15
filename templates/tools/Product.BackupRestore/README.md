# Product.BackupRestore

SQL Server backup, validation, retention, and restore utility.

## Backup

```powershell
$env:PRODUCT_CONNECTION_STRING = "..."
Product.BackupRestore.exe `
  --backup-directory "C:\ProgramData\Company\Product\Backups" `
  --retention-days 30
```

## Validate

```powershell
Product.BackupRestore.exe `
  --validate `
  --backup-file "C:\Backups\ProductDB.bak"
```

## Restore

```powershell
Product.BackupRestore.exe `
  --restore `
  --backup-file "C:\Backups\ProductDB.bak" `
  --confirm-restore
```

Restore always validates the backup first and requires explicit confirmation.
