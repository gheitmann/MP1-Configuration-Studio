# MP1 Configuration Studio

A modern Windows interface for the original MediaPortal 1.39 configurator.

> **Current release:** [1.1.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.1.0) for MediaPortal 1.39 x64. The build is **unsigned** because no code-signing certificate is configured; verify the published SHA256 and expect a possible SmartScreen warning. The Studio shell is German; native page labels follow the MediaPortal language.
>
> `0.1.0-alpha.1` is kept as a historical preview only.

Studio adds grouped navigation with icons, search, navigation history, light/dark themes, adjustable text and page scaling (100–175%, default 125%), and a collapsible help column. The original MediaPortal pages, controls and save handlers remain in use.

## Roadmap and support

- [Roadmap to 1.0](ROADMAP.md)
- [Release checklist](docs/RELEASE-CHECKLIST.md)
- [Support matrix and reporting](SUPPORT.md)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation.

- Committers and reviewers: [@gheitmann](https://github.com/gheitmann)
- Approvers: [@gheitmann](https://github.com/gheitmann)
- Privacy policy: [PRIVACY.md](PRIVACY.md)

The published 1.1.0 build predates the SignPath application and is unsigned. The SignPath application will be submitted after the planned repository move toward the official MediaPortal organization, so that the project is bound to its final URL. A signed 1.1.x re-release will then use the prepared workflow. See [docs/SIGNPATH.md](docs/SIGNPATH.md).

## Install and run

1.1.0 requires Windows 11 23H2 or newer, .NET Framework 4.8, and MediaPortal 1.39 x64. x86 is not shipped.

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download `MP1-Configuration-Studio-1.1.0-Setup.exe` or the portable ZIP from [Releases](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.1.0). Verify the SHA256 sidecar.
4. Run the setup (administrator approval is required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

The original `Configuration.exe` and native DLLs are not replaced. Run `Configuration.exe` to use the original interface. Uninstall through the setup uninstaller or remove only the additional `MP1ConfigurationStudio*` files. Appearance preferences are stored separately under `%LOCALAPPDATA%\MP1 Configuration Studio\appearance.xml`.

The setup and ZIP are unsigned. Windows SmartScreen or antivirus software may warn; the SHA256 sidecars and the source in this repository allow verification.

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

1.1.0 validation: 186/186 fixture checks on x64, 27/27 native smoke checks on isolated MediaPortal 1.39 x64, and a full 175% review of 61/61 pages with 0 value failures and 0 overflow candidates. Attach time is 388 ms. See [final verification](docs/FINAL-1.1.0-VERIFICATION.md) and [release notes](docs/RELEASE-1.1.0.md) for scope and known limitations. The build is unsigned.

## Build

Build with the .NET 8 SDK:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

The build targets .NET Framework 4.8 through the `Microsoft.NETFramework.ReferenceAssemblies` package, produces an x64 application, and runs the fixture checks from the separate `dist\diagnostics\StudioDiagnostics.exe`. The diagnostics host is not part of the shipped application. Output and the test log are written to `dist/`. Native integration tests require an isolated MediaPortal 1.39 x64 runtime; never point persistence tests at a live user profile.

Create a portable release ZIP:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Package-Studio.ps1 -Version 1.1.0
```

The tag release workflow builds the setup executable with Inno Setup, writes SHA256 sidecars, and publishes a draft GitHub release. Final releases require the signing secrets `SIGNING_PFX_BASE64` and `SIGNING_PFX_PASSWORD`.

Architecture details: [ARCHITEKTUR.md](ARCHITEKTUR.md). Source is licensed under [GNU GPL v3](LICENSE). MediaPortal is a separate project; this repository does not redistribute its runtime, plugins, skins or user configuration.
