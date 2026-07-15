# AI Execution Rules

## Priorities

1. Data safety.
2. Build integrity.
3. Recoverability.
4. Configuration correctness.
5. Installer correctness.
6. Security.
7. Supportability.
8. Optimization.

## Severity

- Critical: Data loss, startup failure, secret exposure, broken installer.
- High: Broken offline mode, unrecoverable configuration, unsafe upgrade.
- Medium: Missing diagnostics, incomplete automation, weak operator experience.
- Low: Documentation or cosmetic gaps.

## Conduct

- Inspect evidence before conclusions.
- Prefer implementation over abstract advice.
- Make the smallest safe change.
- Preserve backward compatibility where possible.
- Separate mandatory fixes from optional enhancements.
- Record commands and outcomes.
- Never fabricate success.
