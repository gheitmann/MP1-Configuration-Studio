# MP1 Configuration Studio 2.0.0-beta.33

**Status:** Thirty-third preview. Fixes the MediaPortal plugin's input handling and settings persistence. Unsigned preview. Not installed locally (no UAC run).

## Fixed in beta.33

### Plugin was visible but not usable

The in-MediaPortal plugin relied on MediaPortal moving focus between its row buttons. The focused control was often a row that never changed, so Up/Down appeared to do nothing, Left/Right never edited values, and the footer actions were unreachable.

The plugin now keeps its own focus list:

- **Up/Down** moves through rows, page controls, **Sichern**, and **Zurück/Schließen**
- **Left/Right** changes booleans, combos, and integers
- **OK/Enter** opens sections, toggles values, cycles combos, and opens text input
- **Back/Esc** leaves a section, then closes the plugin back to the MediaPortal home screen
- Focus is applied with `GUI_MSG_SETFOCUS` instead of `SelectControl` (which only selects, not focuses)

### Values changed but labels stayed old

`ShowFields` cached the label text once. `RefreshControls` now recomputes every visible row label, so a toggled value appears immediately and the header shows `ungespeichert`.

### Unrelated settings were reset on MediaPortal exit

The plugin used a long-lived `new MPSettings()` instance. In the live test this led MediaPortal to persist defaults for unrelated `general` entries on shutdown (start/fullscreen/minimize/logging flags).

The plugin now reads and writes `MediaPortal.xml` directly:

- only the edited entries are changed
- a ZIP backup is created before saving
- the file is written atomically
- MediaPortal's shared settings cache is not touched
- saving reports that a MediaPortal restart is required

The affected production configuration was restored from the pre-test backup and verified: watchdog, minimize/fullscreen/logging flags, volume, and all other values match the baseline again.

## Validation in running MediaPortal

- Plugin opened through the idle-plugin mechanism for a deterministic test.
- `Down Down` moved the highlight from **Allgemein** to **Lautstärke**.
- `Enter` opened **Allgemein**; `Down Down Right` changed **Watchdog aktivieren** from **Nein** to **Ja** and showed `ungespeichert` immediately.
- `Sichern` wrote `watchdogEnabled=yes`, created `mp-plugin-20261114-231121-042.zip`, and showed `Gespeichert · Neustart erforderlich · Sicherung: …`.
- `Zurück` returned to the section list; `Esc` closed the plugin back to **MediaPortal - Home** while MediaPortal kept running.
- After a full MediaPortal exit, `watchdogEnabled=yes` was still present — the direct save survives shutdown.
- Test changes were reverted: `watchdogEnabled=no`, idle-plugin test settings removed, volume restored.

## Checks

- Fixture checks: **238/238** on x64.
- Plugin build: 0 warnings, 0 errors.
- `ISetupForm` smoke test: window `8917`, home entry present.

## Install

The combined setup installs the fixed plugin DLL and skin as optional components. The standalone plugin ZIP remains available for manual install/uninstall.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
