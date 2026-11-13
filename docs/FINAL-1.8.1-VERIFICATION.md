# 1.8.1 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.8.1](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.8.1)
**Tag:** `v1.8.1`
**Commit:** `f8411de`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver. SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.8.1-Portable.zip` | 224,188 | `87AE4A08A75982F660B68FA5472BA390DD6AC0DE773646D6386D2DB64CAFF5C1` |
| `MP1-Configuration-Studio-1.8.1-Setup.exe` | 2,486,128 | `AC71EEF9B5E1E02DAB9E201EF74A2FFEEAD6614A5A8814D1CF0699824DA34055` |

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
| Plugin dialogs | 5 / 5 |
| Page screenshots | 61 / 61 |
| Low-contrast control audit | 0 |
| Attach time | about 340 ms |

## 1.8.1 changes

- DVD Discs/Images and Dynamic Refresh Rate use their original native geometry again; the page zoom is disabled for these fragile fixed layouts so labels, checkboxes, and tables no longer collide.
- Pages with `TableLayoutPanel`/`FlowLayoutPanel` skip the geometry scaler and keep their native layout.
- Review diagnostics now capture detail-tab screenshots and layout-panel counts.
- Includes all 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Native layout note

For the fragile pages above, preserving the original layout is safer than forcing a zoom. Text is smaller there; Windows display scaling or the classic interface remains available. Remaining overlap candidates are fixed native checkbox/description bounds, not Studio theme regressions.

## Known limitations

- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI remains pending; mixed-DPI monitor moves are best effort.
- A dedicated visual High Contrast session at OS level remains pending.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks and OS DPI 150/200% fresh-session checks remain pending.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
