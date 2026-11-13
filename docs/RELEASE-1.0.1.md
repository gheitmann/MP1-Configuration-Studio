# MP1 Configuration Studio 1.0.1

**Status:** Maintenance release for MediaPortal 1.39 x64. The build remains unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published with the assets.

## Changes

- **High-DPI default:** On a new installation with OS DPI at 125% or higher, the internal scale now defaults to 100% and native page zoom is not applied a second time. This avoids compounding Windows scaling and the Studio's own scaling on 4K displays.
- **High Contrast:** Custom native theming and owner-draw chrome are disabled while Windows High Contrast is active. Native controls keep their system theme; the shell uses system colors.
- **Preflight:** Studio warns when MediaPortal or the original configurator is still running and asks before continuing.
- **Single instance:** A second Studio instance in the same session is refused with a clear message.
- **Error dialog:** Startup and attach failures offer classic mode, a support bundle, and opening the log, instead of a plain message box.
- **Backup manager:** Lists first-run and manual backups, supports restore and delete, and opens the backup folder.
- **Search:** Result labels use the Studio page names, and combo/list/list-view options remain searchable and selectable.
- **Release packaging:** The package and release workflows select release notes by version instead of hardcoding `1.0.0`.

## Validation

- Fixture checks: 183/183 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Attach time: 338 ms after lazy page preparation.
- Original `Configuration.exe` and production `MediaPortal.xml` hashes unchanged after isolated runs.

## Supported matrix

| Item | Support |
| --- | --- |
| MediaPortal | 1.39 x64 |
| Windows | Windows 11 23H2 or newer |
| Runtime | .NET Framework 4.8 |
| Languages | German shell; native labels follow MediaPortal |
| Internal scaling | 100, 115, 125, 150, 175% |
| OS DPI | 100% validated; 150/200% still need a fresh-session check |
| Plugins | Native pages Tier A; unknown plugins Tier B; Tier C blocked |

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the setup or portable ZIP and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

The original `Configuration.exe` and native DLLs are not replaced.

## Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation. The application will be submitted after the planned repository move toward the official MediaPortal organization, so the SignPath project is bound to the final repository URL.

- Committers and reviewers: [@gheitmann](https://github.com/gheitmann)
- Approvers: [@gheitmann](https://github.com/gheitmann)
- Privacy policy: [PRIVACY.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/PRIVACY.md)
- SignPath setup: [docs/SIGNPATH.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/docs/SIGNPATH.md)

## Known limitations

- Unsigned build until SignPath approval.
- Clean Windows 11 VM installation/start still pending.
- OS DPI 150/200% not yet validated in a fresh session.
- The Studio shell is German; English shell localization is planned for 1.1.
- High Contrast now falls back to native drawing; a dedicated visual audit of every native owner-drawn control is still useful.
- The fixture suite uses the deterministic project harness; NUnit migration is planned.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
