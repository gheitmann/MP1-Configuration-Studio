# MP1 Configuration Studio 2.0.0-beta.25

**Status:** Twenty-fifth preview. Adds automatically generated structured forms for remaining XML entries. Unsigned preview. Not installed locally (no UAC run).

## New in beta.25

Every page now checks its configuration sections for entries that have no explicit or dynamic field and adds a **Weitere Einstellungen / More settings** group automatically.

- `yes`/`no` values become checkboxes.
- other values become text fields.
- Existing explicit fields are excluded, so no setting appears twice.
- Share entries and dynamic plugin lists are excluded because their specialized editors already cover them.
- Plugin pages with full grouped editors (Trakt/OnlineVideos) are detected and skipped.
- Long sections are grouped by their XML section name for orientation.

This closes the remaining legacy XML entries, including advanced general/TV/GUI/audio/video/picture settings, without duplicating the existing forms.

## Validation

- Fixture checks: **224/224** on x64.
- General page inspected: **Weitere Einstellungen** present; `refreshrate01_name` and `AdvancedConfigMode` exposed as fields.
- Save round-trip: `general/refreshrate01_name` changed from `CINEMA` to `TESTCINEMA`, saved, and verified in `MediaPortal.xml`.
- Production configuration untouched.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
