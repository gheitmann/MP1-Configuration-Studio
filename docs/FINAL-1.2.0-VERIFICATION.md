# 1.2.0 — Final Release Verification

**Release:** [MP1 Configuration Studio 1.2.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.2.0)
**Tag:** `v1.2.0`
**Commit:** `4a1019d6dbc0afe2e3d813965f7b6f928fe69b27`
**Published:** 2026-11-13
**Signing:** Unsigned, documented waiver (`ALLOW_UNSIGNED_FINAL=true`). SignPath application deferred until the repository move.

## Published assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.2.0-Portable.zip` | 97,115 | `B4D7DACBD14407302B4B54936B8397E140B229AB8327F0F8D6DB2E047E86CFEA` |
| `MP1-Configuration-Studio-1.2.0-Setup.exe` | 2,188,927 | `7AD3693CB43E499ACC2D46351DB4D0640CE67D8B6737FF7961589240002B12BA` |

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

## 1.2 changes

- Language selection in tools menu (Deutsch/English), persisted and applied after restart.
- English main-shell preview: navigation groups, toolbar, page header, help/status, search, and tools menus.
- Native page labels follow the installed MediaPortal language.
- Includes all 1.1 usability features and 1.0.1 fixes.

## Known limitations

- English is a main-shell preview; onboarding, keyboard help, error, backup, and support-bundle dialogs remain German. Full dialog localization is planned for 1.3.
- Unsigned until SignPath approval.
- High-DPI code path is helper-tested; a real 150/200% session remains pending.
- Per-monitor DPI and NUnit migration are planned for 1.3.
- Clean Windows 11 VM install and elevated setup uninstall remain pending.

## Waiver

The maintainer approved unsigned publication. This document records the waiver; it does not claim that signing, clean-VM, or OS-DPI checks passed.
