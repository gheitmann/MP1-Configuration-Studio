# M2 Status

**Updated:** 2026-11-13
**Branch:** `m1-foundation` (M2 work continues on the same branch)
**Last verification:** 174/174 fixture checks on x64; native smoke 27/27 on isolated MediaPortal 1.39 x64; full 175% review 61/61 with 0 overflow candidates.

## Completed

| Item | Status | Evidence |
| --- | --- | --- |
| Classic mode | Done | `--classic` starts the original `Configuration.exe`; tools menu restarts into classic mode; unsupported MP versions offer the classic fallback |
| Plugin Tier C | Done | Tier C navigation is blocked with a status message and log entry; page setup skips Tier C |
| Configuration backup | Done | ZIP with `MediaPortal.xml` and manifest; first launch creates an isolated first-run backup |
| Configuration restore | Done | Validates `<profile>` XML, creates a safety copy, restores, then closes Studio and restarts classic mode |
| Backup tests | Done | Create, restore, safety copy, missing entry, invalid XML, first-run idempotence |
| High Contrast palette | Done | System colors for shell and custom controls; theme selector disabled while active; owner-draw audit still open |
| Tier A policy | Done | All 61 native sections A/full; unknown plugin sections B/native; C blocked; policy loaded inside the native AppDomain |
| 175% scaling | Done | Full review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs |
| Attach performance | Done | Lazy page preparation reduced attach from 2.7–3.2 s to 303–320 ms, below the 2.0 s budget |
| Search v2 | Done | Combo, list, and list-view options are searchable and selectable |
| Release packaging | Partial | Portable ZIP verified; Inno Setup script and tag release workflow prepared; signing and clean-VM checks pending |
| Native verification | Done | 27/27 after classic, tier, backup, High Contrast, and lazy-preparation changes |

## Decisions

- The product remains a single deployable EXE; Core/Native/App are source boundaries.
- x86 is not shipped in 1.0.
- Tier B keeps the base palette for now; full native theme exclusion is a later refinement.
- Tier A entries are added only after full validation of a page.

## Next M2 items

1. DPI/theme matrix and expand native smoke coverage.
2. Clean Windows 11 VM verification of the CI artifact.
3. Code signing and final release publication.
4. NUnit migration for the fixture suite (deferred from M1-11).
5. High Contrast owner-draw audit for native tabs, lists, and sliders.
6. English shell localization (deferred to 1.1).

## Notes

- First-run backups are written to `%LOCALAPPDATA%\MP1 Configuration Studio\backups` and are excluded from support bundles.
- Backup files contain `MediaPortal.xml` and therefore may contain credentials or service tokens. They are local-only and must not be attached to public issues.
- The classic restart discards unsaved Studio changes by design and warns before proceeding.
