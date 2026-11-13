# MP1 Configuration Studio 1.8.0

**Status:** Readability and native-page audit release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## New in 1.8

- **Tab variant 5:** thin folder tabs with an accent outline on the active tab. All six tabs fit in one row at 100%; at smaller widths or larger scales the strip grows correctly instead of overlapping content.
- **More compact typography:** shell body 10 pt, meta text 9 pt, headings 18 pt, native page target 9 pt. The page header, navigation rows, and tab strip are shorter, so the left menu, tabs, and content no longer compete as strongly.
- **Native page audit and fixes:**
  - Inner native `AutoScroll` is disabled; containers grow to their content and the outer Studio scroll host handles scrolling. This removes the white native scrollbars and reduces clipped controls.
  - Long note/help labels keep their original font size inside the scaled layout, preventing wrapped text from running into checkboxes below.
  - `NumericUpDown` fields keep their scaled width instead of auto-expanding over labels.
  - The decorative GroupBox separator line is removed so it cannot cross overflowing text.
  - `DataGridView` backgrounds, alternating rows, headers, grid lines, and selection colors are themed consistently.
- Ten full-window font comparisons and ten tab variants remain in `docs/mockups/`.
- Includes all 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Validation

- Fixture checks: 207/207 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Page audit: screenshots for all 61 pages, low-contrast control audit, overlap/text-overflow audit.
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

## Design references

- [Tab variants](mockups/README.md) with `tab-styles.png`
- [Full-window font comparisons](mockups/README.md) with `font-styles.png` and `fonts/`
- `shell-100-current.png` and `welcome-100.png`

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
