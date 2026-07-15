# Product.ConfigTool

Safely applies installer-selected settings to runtime JSON.

- Preserves unrelated JSON sections.
- Creates a backup.
- Uses temporary-file and atomic replacement.
- Builds the connection string from structured values.

```powershell
Product.ConfigTool.exe --apply-installer-ini request.ini --silent
```
