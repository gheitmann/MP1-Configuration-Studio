# MP1 Configuration Studio

A modern Windows interface for the original MediaPortal 1.39 configurator.

> **Current stable release:** [1.9.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.9.0) for MediaPortal 1.39 x64. The build is **unsigned** because no code-signing certificate is configured; verify the published SHA256 and expect a possible SmartScreen warning. German and the Studio-owned English UI are complete via Tools → Language. Native page labels follow the MediaPortal language.
>
> **2.0 previews:** [2.0.0-beta.12](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v2.0.0-beta.12) is the new native configuration interface and now the **default** when the application starts without arguments. It reads and writes `MediaPortal.xml` directly, keeps unknown sections and entries, and creates a ZIP backup before saving. Use the sidebar entry **Studio 1.9-Oberfläche** or `--studio` for the 1.9 shell, and `--classic` for the original MediaPortal configurator.
>
> `0.1.0-alpha.1` is kept as a historical preview only.

## New native interface (2.0 beta)

The 2.0 previews replace the re-themed native pages with a dedicated Studio interface:

- Core pages for General, Volume, Pictures, Interface, Start menu, Video, Music, Media folders, TV, Radio, TV recordings, Subtitles, Remote, Codecs, BD/DVD, and Plugins.
- Media-folder editor with add, edit, remove, folder picker, scan toggle, and preservation of pincode/server/login/port and other share fields.
- Dynamic plugin activation lists and a start-menu editor read directly from the configuration.
- Editable 100–175% text size, live settings search, sidebar wheel scrolling, dark scrollbars and title bars.
- Manual ZIP backup, save with automatic pre-save backup, atomic replacement of `MediaPortal.xml`, and keyboard shortcuts (`Ctrl+S`, `Ctrl+W`, `Ctrl+PageUp/PageDown`).
- `--section=<id>` opens a page directly, for example `--section=plugins`.
- Plugin configuration windows still open on the classic interface; the Plugins page links to it.

Studio adds grouped navigation with icons, search, navigation history, light/dark themes, adjustable text and page scaling (100–175%, default 125%), and a collapsible help column. The original MediaPortal pages, controls and save handlers remain in use.

## Roadmap and support

- [Roadmap to 1.0](ROADMAP.md)
- [Release checklist](docs/RELEASE-CHECKLIST.md)
- [Support matrix and reporting](SUPPORT.md)
- [Tab design exploration](docs/mockups/README.md)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation.

- Committers and reviewers: [@gheitmann](https://github.com/gheitmann)
- Approvers: [@gheitmann](https://github.com/gheitmann)
- Privacy policy: [PRIVACY.md](PRIVACY.md)

The published 1.9.0 build predates the SignPath application and is unsigned. The SignPath application will be submitted after the planned repository move toward the official MediaPortal organization, so that the project is bound to its final URL. A signed 1.9.x re-release will then use the prepared workflow. See [docs/SIGNPATH.md](docs/SIGNPATH.md).

## Install and run

1.9.0 requires Windows 11 23H2 or newer, .NET Framework 4.8, and MediaPortal 1.39 x64. x86 is not shipped.

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download `MP1-Configuration-Studio-1.9.0-Setup.exe` or the portable ZIP from [Releases](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.9.0). Verify the SHA256 sidecar.
4. Run the setup (administrator approval is required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

The original `Configuration.exe` and native DLLs are not replaced. Run `Configuration.exe` to use the original interface. Uninstall through the setup uninstaller or remove only the additional `MP1ConfigurationStudio*` files. Appearance preferences are stored separately under `%LOCALAPPDATA%\MP1 Configuration Studio\appearance.xml`.

The setup and ZIP are unsigned. Windows SmartScreen or antivirus software may warn; the SHA256 sidecars and the source in this repository allow verification.

## Configuration safety and classic fallback

- The tools menu can create a backup of `MediaPortal.xml` and restore it later. A first-run backup is created automatically.
- Studio saves a timestamped pre-save backup before the native OK/Apply action and keeps the newest ten. Manual and first-run backups are never pruned automatically; the setting is under Tools → Back up automatically before saving.
- Closing the Studio window with unsaved changes asks for confirmation before discarding them.
- Tools → Restart Studio relaunches Studio. A reported display DPI change also offers an automatic restart so the layout is recalculated; Studio checks the system DPI periodically in case Windows does not raise the form event.
- High Contrast mode uses system colors throughout the custom shell, including hover/selection text, focus, the logo, and scroll thumbs. Support bundles record the system DPI, High Contrast state, and screen count.
- The shell uses a unified Segoe UI type scale with Segoe UI Semibold headings. Body text is 10 pt, meta text 9 pt, headings 18 pt, and native pages target 9 pt at 100%. Section tabs use thin folder outlines with an accent border on the active tab. Inner native scrollbars are replaced by the outer Studio scroll host, long notes keep their original size, and DataGridView rows use the dark palette.
- `MP1ConfigurationStudio.exe --classic` or the tools menu starts the original configurator without the Studio shell.
- Restoring a backup closes Studio, keeps a safety copy of the current file, and restarts the original configurator.

## Navigation

- Ctrl+F: search pages and settings; Enter opens a result.
- Alt+Left / Alt+Right: navigation history.
- Ctrl+Plus / Ctrl+Minus: change size.
- Header controls: standard/expert mode, theme, size and help column.
- OK and Cancel use MediaPortal's original handlers. Plugins and library/database actions may save immediately or separately.

## Validation and limits

1.9.0 validation: 207/207 fixture checks on x64, 27/27 native smoke checks on isolated MediaPortal 1.39 x64, and a full 175% review of 61/61 pages with 0 value failures and 0 overflow candidates. The page audit also captures screenshots, low-contrast controls, and overlap candidates for all 61 pages. Attach time is about 340 ms. See [final verification](docs/FINAL-1.9.0-VERIFICATION.md) and [release notes](docs/RELEASE-1.9.0.md) for scope and known limitations. The build is unsigned.

## Build

Build with the .NET 8 SDK:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

The build targets .NET Framework 4.8 through the `Microsoft.NETFramework.ReferenceAssemblies` package, produces an x64 application, and runs the fixture checks from the separate `dist\diagnostics\StudioDiagnostics.exe`. The diagnostics host is not part of the shipped application. Output and the test log are written to `dist/`. Native integration tests require an isolated MediaPortal 1.39 x64 runtime; never point persistence tests at a live user profile.

Create a portable release ZIP:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Package-Studio.ps1 -Version 1.9.0
```

The tag release workflow builds the setup executable with Inno Setup, writes SHA256 sidecars, and publishes a draft GitHub release. Final releases require the signing secrets `SIGNING_PFX_BASE64` and `SIGNING_PFX_PASSWORD`.

Architecture details: [ARCHITEKTUR.md](ARCHITEKTUR.md). Source is licensed under [GNU GPL v3](LICENSE). MediaPortal is a separate project; this repository does not redistribute its runtime, plugins, skins or user configuration.

The application and setup icon is the original MediaPortal icon from MediaPortal 1.39, used to identify the companion configurator. This project remains unofficial and is not affiliated with Team MediaPortal.
