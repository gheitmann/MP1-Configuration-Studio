# MP1 Configuration Studio 2.0.0-beta.31

**Status:** Thirty-first preview. Adds an update check and fixes the 175 % footer layout. Unsigned preview. Not installed locally (no UAC run).

## New in beta.31

### Update check

- New **Update** button in the footer queries the project's GitHub releases.
- All releases are compared by version (major, minor, patch, beta number), not by API order, because GitHub lists `beta.9` before `beta.30`.
- Stable releases rank above beta releases.
- If a newer release exists, the dialog offers to open the release page.
- If the current build is the newest, the dialog reports it and makes no changes.
- Network errors are reported in the dialog and status bar; no data is sent.

### Command-line check

```powershell
MP1ConfigurationStudio.exe --check-updates
MP1ConfigurationStudio.exe --check-updates 2.0.0-beta.20
```

Prints:

```
current=…
latest=…
url=…
newer=yes|no
```

### 175 % footer

The footer action group now reserves enough width for **Update**, **Sicherung**, **Speichern**, and **Schließen** at 175 % text size. The previous fixed width clipped the new button.

## Validation

- Fixture checks: **238/238** on x64.
- New comparison checks:
  - newer beta vs older beta
  - stable above beta
  - `v` prefix equality
  - numeric components before beta number
- CLI against the live GitHub API:
  - `--check-updates` → `latest=v2.0.0-beta.30`, `newer=no` for the current build
  - `--check-updates 2.0.0-beta.20` → `latest=v2.0.0-beta.30`, `newer=yes`
- UI check: **Update** clicked, dialog showed `Sie verwenden die neueste Version (2.0.0-beta.31).`
- 175 % screenshot verified all four footer buttons visible.
- Production configuration untouched.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
