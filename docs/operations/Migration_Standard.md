# Migration Standard

- Migration files are ordered and immutable after release.
- Every applied migration records a SHA-256 hash.
- Hash mismatches are release blockers.
- Each migration runs in a transaction when supported.
- Back up customer data before applying production migrations.
- Rollback is performed through a new migration or approved database restore.
