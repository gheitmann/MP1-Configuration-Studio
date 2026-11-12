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

## Release workflow and assets

The tag release workflow completed successfully and created a draft release with:

| Asset | Size | SHA256 |
| --- | --- | --- |
| `MP1-Configuration-Studio-1.0.0-rc.1-Portable.zip` | 87,466 | `74EBC9E30A52D5C61B0168F7DDA11320FBF7782DA5B4C6229B7A405136D36701` |
| `MP1-Configuration-Studio-1.0.0-rc.1-Setup.exe` | 2,181,347 | `501B5204E8172D69A68717BBCD2B402C81F0451A5B151ACFE1E0B36F78923441` |

Both SHA256 sidecars match the downloaded assets. The portable ZIP was extracted and reports version 1.0.0.0 with no test/review symbols. The setup executable compiled successfully in CI and is unsigned, as expected for a release candidate.

The local package used for the earlier smoke check has a different hash because it was rebuilt after the release tag with the RC verification document included.

## Pending before final 1.0.0

- Code signing with `SIGNING_PFX_BASE64` and `SIGNING_PFX_PASSWORD`.
- Clean Windows 11 VM installation and start (the setup requires elevation, so it was not executed on the development account).
- Silent setup install/uninstall on an elevated test account.
- OS DPI 150% and 200% fresh-session checks.
- High Contrast owner-draw audit for native tabs, lists, and sliders.
- NUnit migration of the fixture harness (deferred).
- English shell localization (deferred to 1.1).
