# Adaptive Decision Engine

```mermaid
flowchart TD
A[Inspect project] --> B{Application type}
B -->|ASP.NET Core MVC / Blazor Server| C[Kestrel and Launcher]
B -->|API| D[Service or console hosting]
B -->|WinForms / WPF| E[Desktop first-run]
B -->|Blazor WASM| F[Static offline assets]

C --> G{Database}
D --> G
E --> G
G -->|SQLite| H[File database rules]
G -->|LocalDB| I[Single-user SQL rules]
G -->|SQL Express| J[Local server rules]
G -->|SQL Server| K[Central server rules]

H --> L[Publish and installer plan]
I --> L
J --> L
K --> L
```

## Decisions

- Self-contained or framework-dependent.
- Per-user or machine-wide.
- Portable or installed.
- Embedded or server database.
- HTTP or HTTPS.
- Online or offline updates.
- Interactive app or Windows Service.
