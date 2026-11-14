# MP1 Configuration Studio 2.0.0-beta.22

**Status:** Twenty-second preview. Adds friendly forms for small plugin/internal configuration sections. Unsigned preview. Not installed locally (no UAC run).

## New in beta.22

The new **Plugin-Daten / Plugin data** page provides structured forms for:

- `pluginSorting`
- `pluginswindows` (yes/no toggles)
- `Menu0` and `Menu34`
- `IMDbPlus`
- `lastfm:test`
- `FFDShow`
- `mpsettings`
- `Configuration` (configurator window state)

Values use the existing dynamic entry editor: `yes`/`no` become checkboxes, everything else becomes an editable text field. Unknown values and sections remain preserved.

Large plugin-owned sections **Trakt** and **onlinevideos** intentionally remain under **XML-Rohdaten**, because they are large, plugin-managed data sets and change frequently.

## Carried over

- DSP effect-specific Gain/DynAmp/Compressor controls, VST/Winamp discovery, BASS test playback.
- Keymap window add/delete.
- Keymap/views/raw XML add-delete and central save.
- Direct plugin configuration, TV detail lists, 175% fixes.

## Validation

- Fixture checks: **224/224** on x64.
- Plugin-Daten schema page is included in the full page construction test.
- No changes to file formats or save paths.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
