# Security Standard

- Least privilege.
- DPAPI or approved secret protection.
- No secrets in source control.
- No credentials in logs.
- Cookies must match transport.
- CSRF protection.
- Parameterized database access.
- Dependency review.
- Signed updates or integrity verification.
- Data-preserving recovery.

Do not enable HSTS, forced HTTPS, signature enforcement, or anti-debugging unless the deployment supports them.
