# Reference Installer

Use the repository template:

```text
templates/installer/installer.iss
```

Before compiling:

1. Replace `Product` and `Company`.
2. Point `[Files]` to `examples/AspNetCoreMvc.Reference/artifacts`.
3. Generate a unique `AppId`.
4. Ensure the reference AppLauncher is copied into publish output.
5. Package required LocalDB/SQL prerequisites for a fully offline installer.
