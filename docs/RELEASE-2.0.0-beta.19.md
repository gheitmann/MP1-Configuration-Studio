# MP1 Configuration Studio 2.0.0-beta.19

**Status:** Nineteenth preview. Completes the structured editors with add/delete actions, direct plugin configuration, TV detail lists, and 175% layout fixes. Unsigned preview. Not installed locally (no UAC run).

## New in beta.19

### Keyboard editor

- Add actions with action ID, description, key, and sound.
- Delete actions with confirmation.
- Key capture, sound selection, search, and duplicate warning remain.
- Saves `keymap.xml` with ZIP backup and atomic replacement.

### Views editor

- Add and delete views.
- Add and delete filters.
- New filter dialog for field, operator, value, sort, view mode, and limit.
- Existing filter editing, reordering, localized names, and shared-string detachment remain.
- **Validated with MediaPortal's own `SoapFormatter`:** added view/filter and deleted them again; the saved file deserialized into the expected view list.

### Raw XML editor

- Add entries to the selected `MediaPortal.xml` section.
- Delete entries with confirmation.
- Overwrite prompt for existing entry names.
- Values are written through the same safe `StudioConfiguration` layer and only persisted when Studio saves.

### Central save

- The main **Speichern** button / `Ctrl+S` now saves every dirty structured file editor as well:
  - `keymap.xml`
  - `MusicViews.xml` / `VideoViews.xml`
  - `musicdsp.xml`
  - the raw XML file editor
- Each file still creates its own ZIP backup and is replaced atomically.

### Direct plugin configuration

- The **Plugins** page detects configurable plugins from `plugins\Windows` through an isolated helper process.
- Select a plugin and click **Konfigurieren**; the plugin's own configuration window opens directly.
- Verified with **Search music / Global Music Search** and **Home / Home Setup**.
- Classic configuration remains available as fallback.
- Plugin failures stay isolated in the helper process.

### TV Server details

- The TV page now has **Details anzeigen** with read-only lists for:
  - TV channels
  - Radio channels
  - TV groups
  - Radio groups
  - Recordings
  - Schedules/timers
- Counts are corrected to separate TV and radio (16 TV + 4 radio on the reference system).
- Data still comes from the official TV Server business layer through an isolated helper. No writes.

### 175% layout

- Plugin configuration, keyboard, views, DSP, raw XML, and XML file pages no longer lose their input fields or action buttons at 175% text size.
- Save/dropdown widths are proportional; toolbars wrap.

## Validation

- Fixture checks: **224/224** on x64.
- Keymap: added an action with ID/description/key, saved, verified in `keymap.xml`; deleted an action and saved.
- Views: added a view, added a filter, saved, deserialized with `SoapFormatter` (13 views), deleted filter and view, saved (12 views).
- Raw XML: added `TestEntry=42` to `general`, saved, verified; backend add/delete covered by fixture tests.
- Plugins: helper listed **13 configurable plugins**; direct configuration opened Global Music Search and Home Setup windows.
- TV: helper returned 16 TV channels, 4 radio channels, 3 TV groups, 2 radio groups, 72 recordings, 0 schedules, 2 cards; detail dialog inspected.
- 175% screenshots checked for Plugins, Keyboard, and Music views.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
