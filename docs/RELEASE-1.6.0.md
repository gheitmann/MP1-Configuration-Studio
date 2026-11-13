# MP1 Configuration Studio 1.6.0

**Status:** Save-safety release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## New in 1.6

- **Automatic pre-save backup:** Studio writes a timestamped configuration backup before the native OK/Apply save path runs. The newest ten pre-save backups are kept; manual and first-run backups are never pruned automatically.
- **Toggle:** Tools → Back up automatically before saving. Enabled by default.
- **Backup manager:** shows the number and total size of backups and can clean up old automatic pre-save backups while keeping the newest ten.
- Includes all 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Validation

- Fixture checks: 205/205 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Attach time: about 340 ms.

## Supported matrix

| Item | Support |
| --- | --- |
| MediaPortal | 1.39 x64 |
| Windows | Windows 11 23H2 or newer |
| Runtime | .NET Framework 4.8 |
| Languages | German complete; English complete except MediaPortal/plugin-owned dialogs |
| Internal scaling | 100, 115, 125, 150, 175% |
| OS DPI | 100% validated; 150/200% still need a fresh-session check |
| High Contrast | System palette and system-color owner drawing; a dedicated visual session remains pending |
| Plugins | Native pages Tier A; unknown plugins Tier B; Tier C blocked |

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the setup or portable ZIP and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

## Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation. The application will be submitted after the planned repository move toward the official MediaPortal organization.

- Committers and reviewers: [@gheitmann](https://github.com/gheitmann)
- Approvers: [@gheitmann](https://github.com/gheitmann)
- Privacy policy: [PRIVACY.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/PRIVACY.md)
- SignPath setup: [docs/SIGNPATH.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/docs/SIGNPATH.md)

## Known limitations

- Pre-save backups cover the main OK/Apply actions. A plugin-owned save inside a plugin dialog is not intercepted.
- The pre-save backup adds a short disk write before saving; failures are logged and never block the native save.
- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI remains pending; mixed-DPI monitor moves are best effort.
- A dedicated visual High Contrast session at OS level remains pending.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks and OS DPI 150/200% fresh-session checks remain pending.
- Unsigned until SignPath approval.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
