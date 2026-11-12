# M2 Status

**Updated:** 2026-11-13
**Branch:** `m1-foundation` (M2 work continues on the same branch)
**Last verification:** 171/171 fixture checks on x64; native smoke 27/27 on isolated MediaPortal 1.39 x64.

## Completed

| Item | Status | Evidence |
| --- | --- | --- |
| Classic mode | Done | `--classic` starts the original `Configuration.exe`; tools menu restarts into classic mode; unsupported MP versions offer the classic fallback |
| Plugin Tier C | Done | Tier C navigation is blocked with a status message and log entry; page setup skips Tier C |
| Configuration backup | Done | ZIP with `MediaPortal.xml` and manifest; first launch creates an isolated first-run backup |
| Configuration restore | Done | Validates `<profile>` XML, creates a safety copy, restores, then closes Studio and restarts classic mode |
| Backup tests | Done | Create, restore, safety copy, missing entry, invalid XML, first-run idempotence |
| Native verification | Done | 27/27 after backup and tier changes |

## Decisions

- The product remains a single deployable EXE; Core/Native/App are source boundaries.
- x86 is not shipped in 1.0.
- Tier B keeps the base palette for now; full native theme exclusion is a later refinement.
- Tier A entries are added only after full validation of a page.

## Next M2 items

1. High Contrast fallback (system colors, disable custom owner-draw).
2. 175% scaling decision: keep only if all pages pass.
3. Attach performance: reduce 2.7–3.2 s toward the 2.0 s budget.
4. Tier A validation for the core pages.
5. DPI/theme matrix and expand native smoke coverage.
6. Clean Windows 11 VM verification of the CI artifact.
7. NUnit migration for the fixture suite (deferred from M1-11).

## Notes

- First-run backups are written to `%LOCALAPPDATA%\MP1 Configuration Studio\backups` and are excluded from support bundles.
- Backup files contain `MediaPortal.xml` and therefore may contain credentials or service tokens. They are local-only and must not be attached to public issues.
- The classic restart discards unsaved Studio changes by design and warns before proceeding.
