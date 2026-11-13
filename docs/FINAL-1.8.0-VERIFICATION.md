# 1.8.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.8.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.8.0)
**Tag:** `v1.8.0`
**Commit:** `d6f2366`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver. SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.8.0-Portable.zip` | 224,333 | `7A156DAC72DE267D23442A2A35C90CD3D9201C5E5F80CE6CDBFB0AF7348F8452` |
| `MP1-Configuration-Studio-1.8.0-Setup.exe` | 2,486,149 | `90818E9AE19337342A1080342C41B6988E09E32A16B942E58BF37581A5196D92` |

Both sidecars match.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 207 / 207 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 pages, 0 value differences |
| Overflow candidates | 0 |
| Page errors | 0 |
| Plugin dialogs | 5 / 5, cancel kept configuration files unchanged |
| Native page screenshots | 61 / 61 |
| Low-contrast control audit | 0 enabled or disabled text controls below threshold |
| Native page font audit | Segoe UI on every audited page |
| Attach time | about 340 ms |

The page audit also records overlap candidates. Remaining candidates come from MediaPortal's fixed native checkbox/description bounds, not from the Studio theme changes; they are documented as a native-page limitation.

## 1.8 changes

- Tab variant 5: thin folder tabs with an accent outline on the active tab; one row at 100% and correct growth when wrapped.
- Compact type scale: shell body 10 pt, meta 9 pt, headings 18 pt, native pages 9 pt at 100%.
- Shorter navigation rows, page header, and tab strip.
- Inner native AutoScroll disabled; the outer Studio scroll host handles scrolling and removes white native scrollbars.
- Long note/help labels keep their original size; wrapped labels grow and shift following controls to avoid overlaps.
- NumericUpDown fields keep their scaled width instead of covering labels.
- GroupBox separator line removed.
- DataGridView backgrounds, rows, alternating rows, headers, row template, and selection colors themed; semantic cell colors are preserved.
- Includes all 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Known limitations

- Some native MediaPortal pages use fixed absolute layouts. Remaining overlap candidates are native checkbox/description bounds, not Studio theme regressions.
- MovieViews/MusicViews may retain a light native table surface where MediaPortal custom-paints the grid; text contrast remains readable.
- MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI remains pending; mixed-DPI monitor moves are best effort.
- A dedicated visual High Contrast session at OS level remains pending.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks and OS DPI 150/200% fresh-session checks remain pending.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
