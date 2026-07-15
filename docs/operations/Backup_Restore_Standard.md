# Backup and Restore Standard

## Backup

- Use SQL Server CHECKSUM.
- Use COPY_ONLY when the tool must not affect the normal backup chain.
- Verify every generated backup.
- Retain at least one valid backup.
- Store backups outside Program Files.

## Restore

- Require explicit confirmation.
- Validate before restore.
- Stop the application.
- Back up current data before destructive restore when possible.
- Return the database to multi-user mode on failure.
- Run health checks after restore.
