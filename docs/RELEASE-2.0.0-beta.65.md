# MP1 Configuration Studio 2.0.0-beta.65

**Status:** Sixty-fifth preview. Consistent button outlines and clean corners in the footer and dialogs. Unsigned preview.

## UI fix

- The blue primary buttons (Save, OK, and other confirm actions) now draw an outline like the secondary buttons, using a slightly darker shade of their own fill colour. Footers no longer show a bordered Update/Backup/Close next to an edgeless Save.
- `GlassButton` now clears its background before painting. Previously the unpainted right/bottom edge pixels and the rounded corners could retain dark artefacts, which looked like a mismatched black border.
- The same treatment applies to `StudioButton` primary buttons in dialogs, so confirm buttons are visually consistent everywhere.
- Regression tests render a primary button offscreen and pin two properties: the rounded corners clear to the panel background, and the outline is drawn (287 checks).

## Validation

- Fixture checks: **287/287** on x64.
- Builds: x64 and x86 both compiled and PE-verified.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together, choosing the Studio architecture automatically.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal artwork is used for identification only.
