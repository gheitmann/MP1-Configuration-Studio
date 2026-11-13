# 1.1.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.1.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.1.0)
**Tag:** `v1.1.0`
**Commit:** `caa30ddc01a06d2ef8e7ccd72d66b77508eef256`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver (`ALLOW_UNSIGNED_FINAL=true`). SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.1.0-Portable.zip` | 94,288 | `021C727997F0C0F90E4A93BAF524CB64244429413AC9266B6BC73573508A9422` |
| `MP1-Configuration-Studio-1.1.0-Setup.exe` | 2,186,529 | `6A5DC50F994B16D56C12BF62318033EB0377C642ACFC479605C7941E2567E7F0` |

Both sidecars match. The portable ZIP reports FileVersion 1.1.0.0 and contains no test/review symbols.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 186 / 186 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 |
| Overflow candidates | 0 |
| Plugin dialogs | 5 / 5 |
| Attach time | 388 ms |

## 1.1 changes

- First-run onboarding dialog (backup, MediaPortal closed, classic fallback, scaling, search).
- Visible compatibility tier in the page header.
- “Native page size” action for the current page.
- Keyboard shortcut overview via Shift+F1 or the tools menu.
- Defined tab order and visible focus for shell controls.
- Includes all 1.0.1 fixes: High-DPI default, High Contrast fallback, preflight, single instance, actionable error dialog, backup manager.

## Deferred to 1.2

- English shell localization.
- Per-monitor DPI awareness beyond the 1.0.1 high-DPI default.
- NUnit migration of the fixture harness.
- Clean Windows 11 VM install and OS DPI 150/200% checks.

## Known limitations

- Unsigned until SignPath approval after the repository move.
- The high-DPI code path is helper-tested; a real 150/200% session remains pending.
- The shell is German; native page labels follow MediaPortal.
- High Contrast uses native drawing for native controls; a dedicated visual audit is still useful.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, or OS-DPI checks passed.
