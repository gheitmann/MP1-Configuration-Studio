# 1.8.3 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.8.3](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.8.3)
**Tag:** `v1.8.3`
**Commit:** `6055e90`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.8.3-Portable.zip` | 224,246 | `E0F1CDA3208DB04B2E7357CBCFA4A59AE1E9AE9AAD834D4577E89A82989D2F70` |
| `MP1-Configuration-Studio-1.8.3-Setup.exe` | 2,486,155 | `269C898B37D53D632B946814B9F0B57325DAB02A0D2489DD2A285FA9D6D75E68` |

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

## 1.8.3 changes

- Fixed the Tools `ContextMenuStrip` lifetime crash (`ObjectDisposedException`); opening Tools and its submenus no longer disposes a menu that a child still references.
- Implemented tab variant 9: larger fine-card tabs with a thin outline, accent outline and accent text for the active tab.
- The right help column is hidden by default; the top-right help icon or Tools → Show help column restores it. This gives native pages more width and removes several scrollbars.
- Label-like native controls are rendered as readable text even when MediaPortal marks them disabled; they remain non-interactive.
- Includes all 1.8.2, 1.8.1, 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 fixes.

## Known limitations

- Some MediaPortal pages use fixed absolute coordinates; individual controls can still clip or overlap. Hiding the help column helps horizontal space; the classic interface remains the fallback.
- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, and clean-VM checks remain pending.
- NUnit migration is deferred.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
