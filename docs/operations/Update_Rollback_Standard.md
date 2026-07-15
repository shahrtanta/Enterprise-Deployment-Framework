# Update and Rollback Standard

- Stop the application before updating.
- Verify package integrity before extraction.
- Stage updates outside the installation directory.
- Back up the current installation.
- Exclude mutable customer data from binary rollback.
- Apply files only after validation.
- Restore the previous installation on failure.
- Run smoke tests before marking the update successful.
