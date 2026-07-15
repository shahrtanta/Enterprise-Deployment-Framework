# Configuration Tool Standard

The configuration tool is the installer-facing component responsible for runtime JSON updates.

- Preserve unrelated JSON.
- Back up before replacement.
- Use temporary-file and atomic replacement.
- Return non-zero exit codes on failure.
- Never log passwords.
- Store runtime configuration outside Program Files.
