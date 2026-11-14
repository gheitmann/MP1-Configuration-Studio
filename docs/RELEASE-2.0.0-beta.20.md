# MP1 Configuration Studio 2.0.0-beta.20

**Status:** Twentieth preview. Polish release for the structured editors: view search and 175% layout corrections. Unsigned preview. Not installed locally (no UAC run).

## New in beta.20

- **View search:** the Musik-Ansichten and Video-Ansichten pages have a search field above the view list. Typing jumps to the first matching localized view name.
- **Keyboard at 175%:** description, key, clear, sound, and delete columns are proportional; full action descriptions and button labels remain readable.
- **Raw XML at 175%:** section and entry columns are proportional; the add-entry button remains usable and labels no longer push values off-screen.
- **XML file editor at 175%:** toolbar buttons keep their space; the status text is shortened instead of pushing the buttons out.
- **DSP at 175%:** music-folder controls and effect/plugin buttons reflow instead of clipping.
- **Plugin configuration at 175%:** plugin selection gets the full remaining width and action buttons wrap cleanly.

No changes to data formats or save behavior. The add/delete, plugin configuration, TV detail, and central-save features from beta.19 are unchanged.

## Validation

- Fixture checks: **224/224** on x64.
- 175% screenshots inspected for Plugins, Keyboard, and Music views.
- Keymap, views, DSP, XML file, raw XML, plugin list, and TV overview helpers verified in beta.18/beta.19.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
