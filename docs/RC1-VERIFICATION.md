# Release Candidate 1 — Verification

**Tag:** `v1.0.0-rc.1`
**Commit:** `d722893f8110a4ea5d1e388cf313596d3bfd52c3`
**Date:** 2026-11-13
**Environment:** Windows 11 build 26340, 3440×1440, OS DPI 100%, MediaPortal 1.39 x64.

## Automated and isolated results

| Check | Result |
| --- | --- |
| Fixture checks x64 | 174 / 174 |
| Native smoke, isolated MP 1.39 x64 | 27 / 27 |
| Full 175% review | 61 / 61 pages |
| Value preservation across scales/themes | 61 / 61 |
| Overflow candidates | 0 |
| Plugin dialogs | 5 / 5 opened, values preserved, cancel kept config files |
| Attach time | 303–320 ms |
| Classic CLI smoke | Original configurator started |
| Original `Configuration.exe` hash | Unchanged |
| Production `MediaPortal.xml` hash | Unchanged |

## Local package

`MP1-Configuration-Studio-1.0.0-rc.1-Portable.zip`

SHA256:

```text
33D434E59A5569B5145E1743D794658A4D45B66A170FC2BF625C3AE0685846E5
```

## Pending before final 1.0.0

- Code signing with `SIGNING_PFX_BASE64` and `SIGNING_PFX_PASSWORD`.
- Clean Windows 11 VM installation and start.
- OS DPI 150% and 200% fresh-session checks.
- High Contrast owner-draw audit for native tabs, lists, and sliders.
- NUnit migration of the fixture harness (deferred).
- English shell localization (deferred to 1.1).
