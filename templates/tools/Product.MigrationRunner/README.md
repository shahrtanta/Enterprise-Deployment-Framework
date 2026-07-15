# Product.MigrationRunner

Applies ordered SQL scripts and records their SHA-256 hashes.

## Naming

```text
001_initial_schema.sql
002_add_invoice_status.sql
003_create_indexes.sql
```

## Usage

```powershell
$env:PRODUCT_CONNECTION_STRING = "..."
Product.MigrationRunner.exe --scripts ".\Database\Migrations"
```

Applied scripts must never be edited. Create a new migration instead.
