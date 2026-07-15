# Backup, Migration, and Update Checklist

## Backup

- [ ] Backup folder is outside Program Files.
- [ ] Backup uses CHECKSUM.
- [ ] Backup validation succeeds.
- [ ] Retention never removes the newest backup.
- [ ] Secrets are not logged.

## Migration

- [ ] Production backup exists.
- [ ] Scripts are ordered.
- [ ] Applied script hashes match.
- [ ] Transactions are used.
- [ ] Failure stops later migrations.

## Update

- [ ] Application is stopped.
- [ ] Package hash matches.
- [ ] Staging folder is separate.
- [ ] Current binaries are backed up.
- [ ] Mutable data is excluded.
- [ ] Rollback is tested.
- [ ] Post-update smoke test is executed.
