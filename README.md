# Enterprise Deployment Framework (EDF)

A reusable engineering standard and implementation framework for transforming .NET applications into deployment-ready products for clean Windows environments.

## Scope

EDF covers architecture, offline publishing, Windows installation, database bootstrap, first-run configuration, launchers, diagnostics, repair, backup, updates, security, and release validation.

## Core deliverables

- Enterprise Master Prompt
- AI execution rules
- Adaptive decision engine
- Deployment workflow
- Inno Setup standard
- Publish script template
- Operational checklists
- Architecture diagrams

## Usage

1. Open `docs/master-prompt/Enterprise_Master_Prompt.md`.
2. Open or attach the target project.
3. Add project-specific constraints.
4. Ask the AI agent to execute the prompt through implementation and verification.
5. Review the final evidence-based deployment report.

## Status

Version 0.3.0 adds deployable configuration and database-testing tools plus a database-aware Inno Setup wizard.

## Current implementation

- `templates/launcher/Product.AppLauncher`: compilable .NET 8 WinForms launcher.
- `templates/validation/Test-PublishArtifact.ps1`: offline artifact validator.
- `.github/workflows/validate-framework.yml`: automated template validation.

## v0.3 implementation

- `templates/tools/Product.ConfigTool`: safe runtime JSON configuration writer.
- `templates/tools/Product.DbConnectionTester`: silent SQL Server and LocalDB tester.
- `templates/installer/installer.iss`: database wizard with Test Connection.
