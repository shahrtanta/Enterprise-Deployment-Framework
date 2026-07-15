# Versioning and Releases

Use Semantic Versioning:

```text
MAJOR.MINOR.PATCH
```

## Release requirements

- Update `CHANGELOG.md`.
- Update framework version in `README.md`.
- Run GitHub Actions validation.
- Tag the commit.
- Attach the source archive.
- Document known limitations.

## Recommended commands

```bash
git add .
git commit -m "Release v0.2.0"
git tag -a v0.2.0 -m "Enterprise Deployment Framework v0.2.0"
git push origin main --tags
```
