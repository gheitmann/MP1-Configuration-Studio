# 1.8.4 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.8.4](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.8.4)
**Tag:** `v1.8.4`
**Commit:** `7247de5`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.8.4-Portable.zip` | 224,068 | `3C33F6A0A3E1228F29FD73D3EBC7491A061CB8E7983D0F7DFCA37A2043CC6A3B` |
| `MP1-Configuration-Studio-1.8.4-Setup.exe` | 2,486,050 | `8678482E7F0059CCBFD6F1E716CEBEE78D25724A1177F0E23E6F5475D2F6D119` |

Both sidecars match.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 207 / 207 |
| Native smoke | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Overflow / value / page errors | 0 / 0 / 0 |
| Plugin dialogs | 5 / 5 |
| Page screenshots | 61 / 61 |
| Low-contrast control audit | 0 |

## 1.8.4 changes

- The native OK/Save path is marked before the save handler runs. A successful save no longer triggers the unsaved-changes discard prompt.
- The prompt still appears for a real window close (X / Alt+F4) with pending edits.
- Includes all 1.8.3, 1.8.2, 1.8.1, 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 fixes.

## Known limitations

- Some MediaPortal pages use fixed absolute coordinates and can still clip content; the classic interface remains the fallback.
- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, and clean-VM checks remain pending.
- NUnit migration is deferred.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
