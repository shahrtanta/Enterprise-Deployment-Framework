# AppLauncher Standard

## Purpose

A launcher provides a desktop-style entry point for a locally hosted ASP.NET Core application.

## Required behavior

1. Single launcher instance.
2. Port and endpoint validation.
3. Application-specific health probe.
4. No duplicate Kestrel process.
5. Clear conflict message for unrelated listeners.
6. Hidden process startup.
7. Browser launch after readiness.
8. Tray controls.
9. Graceful shutdown.
10. ProgramData logging.

## Security

- Do not execute arbitrary command strings.
- Resolve executable and DLL paths from the installation directory.
- Do not kill processes solely by name.
- Track owned process IDs.
- Bind local desktop apps to loopback unless network access is explicitly required.
