# MP1 Configuration Studio 1.8.2

**Status:** Tab-focus and native-font correction release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## Fixes in 1.8.2

- **No double tab frame:** the active folder tab no longer draws a dotted focus rectangle inside its accent outline. The accent outline remains the active-state indicator; unselected tabs still show a focus rectangle when reached by keyboard.
- **Correct native font on fragile pages:** pages that keep their original geometry (DVD Discs/Images, Dynamic Refresh Rate, and layout-managed pages) now explicitly use the native MediaPortal form font. This prevents inherited shell typography from widening labels into combo boxes and checkboxes. MediaPortal's fixed coordinates can still clip individual controls on these pages; the classic interface remains the fallback for a fully original layout.
- Includes all 1.8.1, 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Validation

- Fixture checks: 207/207 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Page audit: 61 screenshots, low-contrast control audit, overlap diagnostics.
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

## Native layout note

Fragile fixed native layouts keep their original metrics. Text is smaller there than on zoomed pages, and individual native controls may still clip because MediaPortal positions them absolutely. Windows display scaling or the classic interface remains available.

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

- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI remains pending; mixed-DPI monitor moves are best effort.
- A dedicated visual High Contrast session at OS level remains pending.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks and OS DPI 150/200% fresh-session checks remain pending.
- Unsigned until SignPath approval.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
