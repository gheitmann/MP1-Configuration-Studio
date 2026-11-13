# MP1 Configuration Studio 1.7.0

**Status:** Readability and visual-consistency release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## New in 1.7

- **Original MediaPortal icon:** the application, Studio dialogs, setup, and shortcuts now use the original MediaPortal 1.39 icon.
- **Unified typography:** Segoe UI with a consistent type scale for shell body text, labels, dialogs, and native pages; headings use Segoe UI Semibold. At 100% the native page text target is now 10.5 pt instead of 9.5 pt.
- **Readable welcome dialog:** no select-all blue highlight, 10.5 pt body text, shorter copy, and a clearly visible primary button.
- **Calmer section tabs:** compact 30 px soft chips replace the large rounded boxes; the active section uses a subtle chip, an accent dot, and accent text (mockup variant 4).
- **Toolbar fit:** the size selector no longer truncates at 100%.
- **Installer fixes:** setup icon, shortcut icon, and per-release notes instead of the hard-coded 1.0.0 notes.
- Includes all 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Validation

- Fixture checks: 207/207 on x64.
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

## Tab design exploration

Ten compact tab variants are documented in [docs/mockups](mockups/README.md) with `tab-styles.png`; variant 4 (soft chip with accent dot) is implemented. Ten full-window font comparisons are in `font-styles.png` and `fonts/`; Segoe UI with a Segoe UI Semibold heading is implemented.

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
