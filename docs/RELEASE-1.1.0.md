# MP1 Configuration Studio 1.1.0

**Status:** Usability release for MediaPortal 1.39 x64. The build remains unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## New in 1.1

- **First-run onboarding:** a short welcome dialog explains backup, closing MediaPortal, classic fallback, scaling, and search. It is shown once.
- **Visible compatibility tier:** the page header shows Tier A (fully supported), Tier B (native look), or Tier C (blocked).
- **Native page size:** the tools menu can reset the current page to its original geometry if a themed/zoomed page is hard to read.
- **Keyboard help:** Shift+F1 or the tools menu opens a shortcut overview.
- **Tab order and focus:** header, search, navigation, tabs, and footer actions have a defined keyboard order and visible focus.
- Includes all 1.0.1 fixes: High-DPI default scaling, High Contrast native fallback, preflight, single instance, actionable error dialog, backup manager, and version-aware packaging.

## Validation

- Fixture checks: 186/186 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Attach time: about 388 ms.
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

## Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation. The application will be submitted after the planned repository move toward the official MediaPortal organization.

- Committers and reviewers: [@gheitmann](https://github.com/gheitmann)
- Approvers: [@gheitmann](https://github.com/gheitmann)
- Privacy policy: [PRIVACY.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/PRIVACY.md)
- SignPath setup: [docs/SIGNPATH.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/docs/SIGNPATH.md)

## Deferred to 1.2

- English shell localization.
- Per-monitor DPI awareness beyond the 1.0.1 high-DPI default fix.
- NUnit migration of the fixture harness.
- Clean Windows 11 VM install and OS DPI 150/200% validation.

## Known limitations

- Unsigned build until SignPath approval.
- High-DPI behavior is helper-tested; a real 150/200% session is still required.
- The Studio shell is German; native page labels follow MediaPortal.
- High Contrast now uses native drawing for native controls; a dedicated visual audit is still useful.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
