# 1.0.1 Release Candidate Verification

**Tag:** `v1.0.1-rc.1`
**Commit:** `a55de7b2f5e0b05eff23a6b8a031a06e2cccaf1b`
**Date:** 2026-11-13
**Environment:** Windows 11 build 26340, 3440×1440, OS DPI 100%, MediaPortal 1.39 x64.

## Results

| Check | Result |
| --- | --- |
| Fixture checks x64 | 177 / 177 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation | 61 / 61 |
| Overflow candidates | 0 |
| Plugin dialogs | 5 / 5 |
| Attach time | 322 ms |
| High-DPI default helper | 96 dpi → 125%, 144 dpi → 100% |
| High Contrast native fallback | Tested: custom theming disabled, native control color preserved |
| Original `Configuration.exe` hash | Unchanged |
| Production `MediaPortal.xml` hash | Unchanged |

## Draft release assets

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.0.1-rc.1-Portable.zip` | 89,240 | `1EAD192DAA1E3A1FB55E90A54C06E2FD535ABA2DC223CC12FCB0AB1707F1A0B0` |
| `MP1-Configuration-Studio-1.0.1-rc.1-Setup.exe` | 2,182,344 | `3B4E5AE4C3C968365F39DB4AC68E2225A3FF746A7AF8DFB5B686463FF15B0348` |

Both SHA256 sidecars match. The portable ZIP reports FileVersion 1.0.1.0 and contains no test/review symbols. The release is a **draft**, unsigned, and not public.

## Pending before final 1.0.1

- OS DPI 150/200% fresh-session verification.
- Clean Windows 11 VM installation/start.
- Repository move and SignPath Foundation approval.
- Final signed `v1.0.1` publication.

The high-DPI code path is covered by the helper test and code review, but the development machine runs at 100% OS DPI, so a real 150% session remains required.
