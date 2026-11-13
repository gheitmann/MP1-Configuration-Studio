# 1.3.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.3.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.3.0)
**Tag:** `v1.3.0`
**Commit:** `80d28fe726ef7a9adf9eb3961d4bc1c9539131ea`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver. SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.3.0-Portable.zip` | 98,896 | `F65326BC33678A18A38A980666663FB0A553F8B5CA6A2FC2565F04665826D53E` |
| `MP1-Configuration-Studio-1.3.0-Setup.exe` | 2,190,372 | `812B9F87F0A938CC674D71264C5AF21F785C277A44FFB1B584BD80ABE435016F` |

Both sidecars match.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 188 / 188 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 |
| Overflow candidates | 0 |
| Plugin dialogs | 5 / 5 |
| Attach time | about 388 ms |

## 1.3 changes

- Full English localization of Studio-owned dialogs and messages: onboarding, keyboard shortcuts, error dialog, backup manager, support bundle, preflight, startup/attach errors, About, restore/classic confirmations.
- Language selection under Tools → Language; restart required.
- DPI-change notice asking for a restart so layout is recalculated.
- Includes all 1.2, 1.1, and 1.0.1 features.

## Known limitations

- MediaPortal and third-party plugin dialogs use their own language/layout.
- Full per-monitor DPI is still pending; Studio detects a DPI change and recommends a restart.
- NUnit migration remains pending.
- Clean Windows 11 VM install and OS DPI 150/200% checks remain pending.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, or OS-DPI checks passed.
