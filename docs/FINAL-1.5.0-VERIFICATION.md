# 1.5.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.5.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.5.0)
**Tag:** `v1.5.0`
**Commit:** `3fe9b6b`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver. SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.5.0-Portable.zip` | 101,270 | `71DB064F220014B7C918E18A13EBEF21BC76457DE1A59F216CA49FB85C2650C1` |
| `MP1-Configuration-Studio-1.5.0-Setup.exe` | 2,192,184 | `EFADA142C986775CF0371CCDE01F8468FEEBC25A6BAAC5CF697F176BF9E916DE` |

Both sidecars match.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 203 / 203 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 pages, 0 value differences |
| Overflow candidates | 0 |
| Page errors | 0 |
| Plugin dialogs | 5 / 5, cancel kept configuration files unchanged |
| Attach time | about 340 ms |

The full review ran from the separate diagnostics build. Automated review/verify runs suppress the unsaved-changes prompt so they can close after exercising native pages.

## 1.5 changes

- System-DPI check every two seconds in addition to the Windows `DpiChanged` event; a real change offers an immediate restart.
- High Contrast owner-draw audit: action text, logo text, focus rectangle, search icon, and scroll thumbs use system colors.
- Support bundle records system DPI, High Contrast state, and screen count.
- Includes all 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Known limitations

- MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI remains pending; mixed-DPI monitor moves are best effort.
- A dedicated visual High Contrast session at OS level remains pending; the color paths are covered by the fixture checks.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks and OS DPI 150/200% fresh-session checks remain pending.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
