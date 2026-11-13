# 1.8.2 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.8.2](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.8.2)
**Tag:** `v1.8.2`
**Commit:** `fea1e61`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.8.2-Portable.zip` | 224,197 | `DCCFC4C8559FA626A204F040A8BCD8170B1FDCA04B3C7FA9897C903C6F3E9A46` |
| `MP1-Configuration-Studio-1.8.2-Setup.exe` | 2,486,139 | `442955B351B07E863B78530DEFF3C22C304B2E728679D887DE102FC8CDEB3FED` |

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

## 1.8.2 changes

- Active folder tabs no longer show a dotted focus rectangle inside the accent outline; unselected tabs keep a keyboard focus indicator.
- Fragile pages (DVD Discs/Images, Dynamic Refresh Rate) use the native MediaPortal form font and original geometry instead of inheriting shell typography.
- Layout-managed pages skip the geometry scaler.
- Includes all 1.8.1, 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Known limitations

- MediaPortal's fixed native coordinates can still clip individual controls on fragile pages; the classic interface remains the fallback for a fully original layout.
- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, and clean-VM checks remain pending.
- NUnit migration is deferred.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
