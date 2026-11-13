# 1.7.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.7.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.7.0)
**Tag:** `v1.7.0`
**Commit:** `f14fd53`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver. SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.7.0-Portable.zip` | 222,694 | `0C36DE9D77B6C21A803C61F5998073D77447D576FD40AF53834D3560933DF377` |
| `MP1-Configuration-Studio-1.7.0-Setup.exe` | 2,484,847 | `14E5EBDCADC12F402CC0A109AEF960D48CB89DED4CCA4B0AD37DC1CBB9410758` |

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
| Native page font audit | Segoe UI on every audited page |
| Attach time | about 340 ms |

## 1.7 changes

- Original MediaPortal icon in the application, Studio dialogs, setup, and shortcuts.
- Unified Segoe UI type scale; Segoe UI Semibold headings; native page target 10.5 pt at 100%.
- Readable first-run dialog without select-all highlight, with a visible primary button.
- Tab variant 4: compact soft chip with accent dot; all tabs fit in one row at 100%.
- Toolbar size selector no longer truncates.
- Installer packages the correct per-version release notes and the original icon.
- Ten tab and ten full-window font mockups under `docs/mockups/`.

## Known limitations

- MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI remains pending; mixed-DPI monitor moves are best effort.
- A dedicated visual High Contrast session at OS level remains pending.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks and OS DPI 150/200% fresh-session checks remain pending.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
