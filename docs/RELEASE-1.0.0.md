# MP1 Configuration Studio 1.0.0

**Status:** 1.0.0 release for MediaPortal 1.39 x64. This build is **unsigned**: no code-signing certificate is configured. Windows SmartScreen may warn; verify the published SHA256 before running.

## Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation. The SignPath application is prepared but not yet approved; this 1.0.0 build predates it.

- Committers and reviewers: [@gheitmann](https://github.com/gheitmann)
- Approvers: [@gheitmann](https://github.com/gheitmann)
- Privacy policy: [PRIVACY.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/PRIVACY.md)
- SignPath setup: [docs/SIGNPATH.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/docs/SIGNPATH.md)

## What it is

A modern, readable shell around the original MediaPortal 1.39 configurator. The native pages, controls, values, and save handlers remain in use.

## Features

- Grouped icon navigation, search, history, and keyboard shortcuts.
- Light/dark themes, internal scaling 100–175%, collapsible help.
- Classic fallback via `--classic` or the tools menu.
- Tier A/B/C compatibility policy for native pages and plugins.
- Configuration backup and restore for `MediaPortal.xml`.
- First-run configuration backup.
- High Contrast system palette.
- Structured logs and a sanitized support bundle.
- About dialog with version and log path.

## Supported matrix

| Item | Support |
| --- | --- |
| MediaPortal | 1.39 x64 |
| Windows | Windows 11 23H2 or newer |
| Runtime | .NET Framework 4.8 |
| Languages | German shell; native labels follow MediaPortal |
| Internal scaling | 100, 115, 125, 150, 175% |
| OS DPI | 100% validated; 150/200% still need a clean-session check |
| Plugins | Tier A native pages; Tier B unknown plugins in native look; Tier C blocked |

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Run the setup or extract the portable ZIP completely.
4. Start `MP1ConfigurationStudio.exe`.
5. Use the tools menu for backup, restore, support bundle, or classic mode.

The original `Configuration.exe` and native DLLs are not replaced. Remove the additional `MP1ConfigurationStudio*` files to uninstall the portable build.

## Validation at 1.0.0

- 173/173 fixture checks on x64.
- Native smoke 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Attach time: 303–320 ms after lazy page preparation.
- Original `Configuration.exe` and production `MediaPortal.xml` hashes unchanged after isolated runs.
- Hosted CI runs 1–16 successful at the time of this document.

## Known limitations

- This release is unsigned. Verify SHA256 and expect a possible SmartScreen warning.
- A clean Windows 11 VM install/start was not executed; validation used the development Windows 11 installation and an isolated MediaPortal runtime.
- OS DPI 150/200% has not been validated in a fresh session.
- Third-party plugins outside the known set remain Tier B and keep their native look.
- High Contrast uses system colors, but native owner-drawn tabs/lists/sliders still need an audit.
- The Studio shell is German in 1.0. Native page labels follow the installed MediaPortal language; English shell localization is planned for 1.1.
- The fixture suite still uses the project's deterministic harness; NUnit migration is planned.

## Privacy

The Studio works offline. It writes appearance preferences, logs, backups, and its own files. It never writes `MediaPortal.xml` except through the native handlers. Backups contain `MediaPortal.xml` and may contain credentials; they are local-only and excluded from support bundles.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
