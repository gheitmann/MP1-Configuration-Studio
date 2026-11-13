# M2 Status

**Updated:** 2026-11-13
**Branch:** `main`
**Release:** [1.6.0](https://github.com/gheitmann/MP1-Configuration-Studio/releases/tag/v1.6.0) published unsigned with a documented waiver. See [FINAL-1.6.0-VERIFICATION.md](FINAL-1.6.0-VERIFICATION.md).
**Last verification:** 205/205 fixture checks on x64; native smoke 27/27 on isolated MediaPortal 1.39 x64; full 175% review 61/61 with 0 overflow candidates.

## Completed

| Item | Status | Evidence |
| --- | --- | --- |
| Classic mode | Done | `--classic` starts the original `Configuration.exe`; tools menu restarts into classic mode; unsupported MP versions offer the classic fallback |
| Plugin Tier C | Done | Tier C navigation is blocked with a status message and log entry; page setup skips Tier C |
| Configuration backup | Done | ZIP with `MediaPortal.xml` and manifest; first launch creates an isolated first-run backup |
| Configuration restore | Done | Validates `<profile>` XML, creates a safety copy, restores, then closes Studio and restarts classic mode |
| Backup tests | Done | Create, restore, safety copy, missing entry, invalid XML, first-run idempotence |
| High Contrast palette | Done | System colors for shell and custom controls; owner-draw audit covers action text, logo, focus, search icon, and scroll thumbs |
| Tier A policy | Done | All 61 native sections A/full; unknown plugin sections B/native; C blocked; policy loaded inside the native AppDomain |
| 175% scaling | Done | Full review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs |
| Attach performance | Done | Lazy page preparation reduced attach from 2.7-3.2 s to about 340 ms, below the 2.0 s budget |
| Display changes | Done | Windows DPI event plus a two-second system-DPI check offer a restart after scaling changes |
| Pre-save backup | Done | Timestamped backup before native OK/Apply; newest ten pre-save files retained; manual and first-run backups untouched |
| High Contrast audit | Done | Selected/hover text, logo text, focus rectangle, search icon, and scroll thumbs use system colors |
| Search v2 | Done | Combo, list, and list-view options are searchable and selectable |
| Release packaging | Done (waived) | 1.0.0 published with portable ZIP, Inno setup, matching SHA256 sidecars; unsigned and clean-VM checks waived |
| Native verification | Done | 27/27 after classic, tier, backup, High Contrast, and lazy-preparation changes |

## Decisions

- The product remains a single deployable EXE; Core/Native/App are source boundaries.
- x86 is not shipped in 1.0.
- Tier B keeps the base palette for now; full native theme exclusion is a later refinement.
- Tier A entries are added only after full validation of a page.

## Next M2 items

1. OS DPI 150/200% and mixed-monitor fresh-session checks.
2. Clean Windows 11 VM verification of the published artifact.
3. Code signing for a future signed re-release (current 1.6.0 is unsigned by waiver).
4. NUnit migration for the fixture suite (deferred from M1-11).
5. A dedicated visual High Contrast session at OS level.

## Notes

- First-run backups are written to `%LOCALAPPDATA%\MP1 Configuration Studio\backups` and are excluded from support bundles.
- Backup files contain `MediaPortal.xml` and therefore may contain credentials or service tokens. They are local-only and must not be attached to public issues.
- The classic restart discards unsaved Studio changes by design and warns before proceeding.
