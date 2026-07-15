# Reference Integration Guide

## Replace product identity

Change:

- `Company`
- `ProductReference`
- `Product.Reference.Web`
- Port `5080`
- Database name

## Integrate your business application

1. Copy `BootstrapPaths`, `DirectoryBootstrap`, and runtime configuration logic.
2. Add the runtime JSON to configuration before building the application.
3. Register health checks.
4. Add `FirstRunMiddleware` before normal route execution.
5. Keep setup routes and local assets accessible.
6. Replace the reference database model with your provider-specific implementation.
7. Run migrations only after connection validation and backup.
8. Publish your real AppLauncher and deployment tools.
9. Update Inno Setup identities, paths, and AppId.
10. Execute smoke tests on a clean Windows environment.
