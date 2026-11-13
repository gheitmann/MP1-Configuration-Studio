# 1.4.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.4.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.4.0)
**Tag:** `v1.4.0`
**Commit:** `b547cca`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver. SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.4.0-Portable.zip` | 100,141 | `34E99F49714873BC8DEB957455BF53BE31143B86F8BA4012B6465421E89DA674` |
| `MP1-Configuration-Studio-1.4.0-Setup.exe` | 2,191,334 | `32110EA02D7282891F632EA27BF2800CCFE321F729A6E3BA2D50D4E55E597D57` |

Both sidecars match.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 197 / 197 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 pages, 0 value differences |
| Overflow candidates | 0 |
| Page errors | 0 |
| Plugin dialogs | 5 / 5 |
| Attach time | about 340 ms |

The full review ran from the separate diagnostics build. Automated review/verify runs suppress the unsaved-changes prompt so they can close after exercising native pages.

## 1.4 changes

- Unsaved-changes guard when closing the Studio window; defaults to keeping Studio open.
- Automatic restart offer after a reported display DPI change.
- Manual Tools → Restart Studio action with an unsaved-changes warning.
- Review runner now uses `StudioDiagnostics.exe` automatically when present.
- Includes all 1.3, 1.2, 1.1, and 1.0.1 features.

## Known limitations

- MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI is still pending; OS DPI 150/200% fresh-session checks remain pending.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks remain pending.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, or OS-DPI checks passed.
