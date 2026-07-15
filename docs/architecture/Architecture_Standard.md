# Architecture Standard

## Principles

- Domain logic is independent of UI, database, and installer concerns.
- Application services define use cases.
- Infrastructure implements external integrations.
- Deployment tooling is isolated.
- Runtime paths use one tested abstraction.
- Configuration is validated before dependent services start.

## Recommended layout

```text
src/
  Product.Domain/
  Product.Application/
  Product.Infrastructure/
  Product.Persistence/
  Product.Web/
tools/
  Product.AppLauncher/
  Product.Diagnostics/
  Product.Repair/
  Product.ConfigTool/
installer/
tests/
```
