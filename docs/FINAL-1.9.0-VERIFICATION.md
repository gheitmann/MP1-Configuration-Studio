# 1.9.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.9.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.9.0)
**Tag:** `v1.9.0`
**Commit:** `5cd239e`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.9.0-Portable.zip` | 224,679 | `522EE7813E3A8684C9BB27B49721D9D04EFD9C6D78EB022D71E8BF6E11D726D4` |
| `MP1-Configuration-Studio-1.9.0-Setup.exe` | 2,486,557 | `9F2819216D4966A4C64664A7C5C5108E9E5E55402678782402E941AD1E7F4E48` |

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
| Low-contrast controls | 0 |
| Overlap candidates | 2 (native fixed layout) |

## 1.9.0 changes

- Native pages use the original MediaPortal look by default; the shell remains dark. This removes black-on-dark text and most fixed-layout collisions caused by re-theming and scaling native controls.
- Tools menu lifetime crash fixed.
- Tab variant 9: larger fine-card tabs.
- Help column hidden by default; restore via top-right icon or Tools.
- Native OK/Save no longer triggers the unsaved-changes discard prompt.
- Includes all 1.8.4, 1.8.3, 1.8.2, 1.8.1, 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 fixes.

## Trade-off

Original native layouts are smaller than the previous zoomed pages. This is intentional: readable control colors and no fixed-coordinate collisions take priority. The Studio-themed native mode remains available under Tools and requires a restart.

## Known limitations

- Some native pages can still clip individual fixed controls; classic interface remains available.
- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, and clean-VM checks remain pending.
- NUnit migration is deferred.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
