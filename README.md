# MP1 Configuration Studio

A modern Windows interface for the original MediaPortal 1.39 configurator.

> **Project status:** `0.1.0-alpha.1` is a historical preview. Development follows the [roadmap to 1.0](ROADMAP.md). Version 1.0.0 is the supported target; no further alpha releases are planned.

Studio adds grouped navigation with icons, search, navigation history, light/dark themes, adjustable text and page scaling (100–175%, default 125%), and a collapsible help column. The original MediaPortal pages, controls and save handlers remain in use.

## Roadmap and support

- [Roadmap to 1.0](ROADMAP.md)
- [Release checklist](docs/RELEASE-CHECKLIST.md)
- [Support matrix and reporting](SUPPORT.md)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## Install and run

The historical alpha requires Windows, .NET Framework 4.7.2 or newer, and an existing MediaPortal 1.39 installation. The 1.0 target requires .NET Framework 4.8 and supports MediaPortal 1.39 x64; x86 will not be shipped.

1. Back up your MediaPortal configuration before trying this historical preview.
2. Download the release ZIP and extract all files together.
3. Run `MP1ConfigurationStudio.exe`. Keep the executable and its configuration files alongside it.
4. On first launch, Studio adds its own files beside the installed `Configuration.exe`; Windows may request administrator approval for this copy.

The original `Configuration.exe` and native DLLs are not replaced. Run `Configuration.exe` to use the original interface. To remove Studio, close it and remove only the additional `MP1ConfigurationStudio*` files. Appearance preferences are stored separately under `%LOCALAPPDATA%\MP1 Configuration Studio\appearance.xml`.

## Configuration safety and classic fallback

- The tools menu can create a backup of `MediaPortal.xml` and restore it later. A first-run backup is created automatically.
- `MP1ConfigurationStudio.exe --classic` or the tools menu starts the original configurator without the Studio shell.
- Restoring a backup closes Studio, keeps a safety copy of the current file, and restarts the original configurator.

## Navigation

- Ctrl+F: search pages and settings; Enter opens a result.
- Alt+Left / Alt+Right: navigation history.
- Ctrl+Plus / Ctrl+Minus: change size.
- Header controls: standard/expert mode, theme, size and help column.
- OK and Cancel use MediaPortal's original handlers. Plugins and library/database actions may save immediately or separately.

## Validation and limits

See [VALIDATION.md](VALIDATION.md) for the measured results and remaining coverage. The `0.1.0-alpha.1` build is an unsigned historical preview. Hardware-dependent features, every third-party plugin, and every Windows display configuration still require broader testing. Some native page labels remain English. Standalone plugin and Windows dialogs retain their own layout. Final validation evidence will be published with 1.0.0.

## Build

Build with the .NET 8 SDK:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

The build targets .NET Framework 4.8 through the `Microsoft.NETFramework.ReferenceAssemblies` package, produces an x64 application, and runs the fixture checks from the separate `dist\diagnostics\StudioDiagnostics.exe`. The diagnostics host is not part of the shipped application. Output and the test log are written to `dist/`. Native integration tests require an isolated MediaPortal 1.39 x64 runtime; never point persistence tests at a live user profile.

Create a portable release ZIP:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Package-Studio.ps1 -Version 1.0.0
```

The tag release workflow builds the setup executable with Inno Setup, writes SHA256 sidecars, and publishes a draft GitHub release. Final releases require the signing secrets `SIGNING_PFX_BASE64` and `SIGNING_PFX_PASSWORD`.

Architecture details: [ARCHITEKTUR.md](ARCHITEKTUR.md). Source is licensed under [GNU GPL v3](LICENSE). MediaPortal is a separate project; this repository does not redistribute its runtime, plugins, skins or user configuration.
