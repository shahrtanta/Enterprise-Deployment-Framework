# Product.Update

Applies a verified offline update package and restores the previous installation if copying fails.

## Manifest

```json
{
  "Product": "Product",
  "Version": "1.2.0",
  "PackageFile": "Product-1.2.0.zip",
  "Sha256": "HEX_SHA256",
  "MinimumVersion": "1.0.0",
  "ReleaseNotes": "..."
}
```

## Usage

```powershell
Product.Update.exe `
  --manifest ".\update-manifest.json" `
  --install-directory "C:\Program Files\Company\Product" `
  --working-directory "C:\ProgramData\Company\Product\Updates" `
  --confirm-apply
```

The application must be stopped before applying an update.
