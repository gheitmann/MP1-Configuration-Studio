# 1.6.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.6.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.6.0)
**Tag:** `v1.6.0`
**Commit:** `73332b3`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver. SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.6.0-Portable.zip` | 102,800 | `37B4025B77AECD6FE3F31EAFD5F16A3241CA5667A8FF080C89B43B53D72AA6F7` |
| `MP1-Configuration-Studio-1.6.0-Setup.exe` | 2,193,438 | `DE0CC5254C1851FDDD82552217EEF1EF9B3E9E519CC8EC8BB87672571B0EC68F` |

Both sidecars match.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 205 / 205 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 pages, 0 value differences |
| Overflow candidates | 0 |
| Page errors | 0 |
| Plugin dialogs | 5 / 5, cancel kept configuration files unchanged |
| Attach time | about 340 ms |

## 1.6 changes

- Automatic timestamped backup before the native OK/Apply save path; the newest ten pre-save backups are kept.
- Manual and first-run backups are never pruned automatically.
- Tools → Back up automatically before saving; enabled by default.
- Backup manager shows count and total size and can clean up old pre-save backups.
- Includes all 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 features.

## Known limitations

- Pre-save backups cover the main OK/Apply actions; plugin-owned save actions are not intercepted.
- A pre-save backup failure is logged and never blocks the native save.
- MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI remains pending; mixed-DPI monitor moves are best effort.
- A dedicated visual High Contrast session at OS level remains pending.
- NUnit migration is deferred; the existing fixture and native smoke harnesses remain the release gate.
- Clean Windows 11 VM install checks and OS DPI 150/200% fresh-session checks remain pending.
- Unsigned until SignPath approval.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, OS-DPI, or a visual High Contrast session passed.
