# 1.0.1 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.0.1](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.0.1)
**Tag:** `v1.0.1`
**Commit:** `698047afe6e70d6043bb121006597c6138e78eca`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver (`ALLOW_UNSIGNED_FINAL=true`). SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.0.1-Portable.zip` | 93,252 | `741CE8CE529C2F53E4DB3D379E1A93BDF256559CA2E467E927A94A777CD63FBC` |
| `MP1-Configuration-Studio-1.0.1-Setup.exe` | 2,185,380 | `9654593A19122844F351147DE0844B383BC7BA1FEC0D11F4D06E8931E51F4B52` |

Both sidecars match. The portable ZIP reports FileVersion 1.0.1.0, contains no test/review symbols, and the extracted published artifact started successfully against the isolated MediaPortal 1.39 x64 runtime.

## Validation

| Check | Result |
| --- | --- |
| Fixture checks x64 | 183 / 183 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 |
| Overflow candidates | 0 |
| Plugin dialogs | 5 / 5 |
| Attach time | 338 ms |
| Published portable artifact start | Passed |
| Original `Configuration.exe` hash | Unchanged |
| Production `MediaPortal.xml` hash | Unchanged |

## 1.0.1 changes

- High-DPI default scaling avoids double scaling for new installations at 125% OS DPI or higher.
- High Contrast disables custom native theming and owner-draw chrome.
- Preflight warning when MediaPortal or the original configurator is running.
- Single-instance protection.
- Actionable error dialog with classic mode, support bundle, and log access.
- Backup manager for listing, restoring, deleting, and opening backups.
- Search result labels use Studio page names; combo/list/list-view options are searchable.
- Version-aware release notes and privacy file included in packages.

## Known limitations

- Unsigned until SignPath approval after the repository move.
- The high-DPI code path is helper-tested; a real 150/200% OS DPI session remains pending.
- Clean Windows 11 VM install/start and elevated setup uninstall remain pending.
- The Studio shell is German; English shell localization is planned for 1.1.
- High Contrast uses native drawing for native controls; a dedicated visual audit is still useful.
- The fixture suite still uses the project harness; NUnit migration is planned.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, or OS-DPI checks passed.
