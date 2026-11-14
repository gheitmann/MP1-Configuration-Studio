# MP1 Configuration Studio 2.0.0-beta.17

**Status:** Seventeenth preview. Adds native editors for keyboard mapping, music/video views, and music DSP, plus a TV Server status panel. Unsigned preview. Not installed locally (no UAC run).

## New in beta.17

### Keyboard editor

The **Tastatur / Keyboard** page edits `keymap.xml` natively:

- Context list for **Global** and every MediaPortal window (description + window ID).
- Search across action description, action ID, and key.
- **Press a key** dialog: the next key press is captured and written to the mapping.
- Sound selector with `click.wav`, `back.wav`, `cursor.wav`, `photo.wav`, empty, and custom values.
- Clear-key button, duplicate-key warning within the same context.
- ZIP backup and atomic replacement on save; unknown actions and attributes are preserved.

### Music and video views

New **Musik-Ansichten** and **Video-Ansichten** pages edit `MusicViews.xml` and `VideoViews.xml`:

- View list with names resolved from the active MediaPortal language file, e.g. **Album Artists · #528** instead of `#528`.
- Reorder views with ▲/▼.
- Every filter is a card: field, operator, value, limit, sort field, view mode, ascending, distinct, and skip-level.
- Saves preserve the .NET SOAP object graph. Shared strings are detached safely, so editing one filter does not silently change other filters.
- **Validated with MediaPortal's own `SoapFormatter`:** the saved files deserialize into the expected `ViewDefinition` list and filter values.
- ZIP backup and atomic replacement.

### Music DSP

New **Musik-DSP** page edits `musicdsp.xml`:

- Music folder with folder picker.
- BASS effects with name and parameter list.
- VST and Winamp plugins with DLL picker and parameters.
- Add/remove effects, plugins, and parameters.
- ZIP backup and atomic replacement.

### TV Server

The **TV** page now starts with a **TV-Server-Status** group:

- Detects the TV Server installation and the `TVService` state.
- Shows provider, database, and server from `Gentle.config` (without passwords).
- Opens **SetupTv** or the TV Server folder directly.

A complete replacement of `SetupTv` is **not** included. Channels, EPG, cards, CAM, schedules, channel scanning, and the TV database are managed by the TV Server service and its own administration application. Reimplementing that safely is a separate server project, not an XML configuration page. The Studio panel makes the status visible and removes the search for SetupTv, but keeps the proven TV Server tooling in charge of the TV subsystem.

## Validation

- Fixture checks: **220/220** on x64.
- Keymap: captured `F8` for an action, saved, backup created; `keymap.xml` valid.
- Views: changed a filter value, verified other filters kept `*`, saved, and deserialized the result with MediaPortal's `SoapFormatter` (12 music views, 15 video views).
- View order: moved a video view down, saved, and verified the new order through `SoapFormatter`.
- DSP: changed the music folder, saved, backup created; XML remains loadable by the DSP settings serializer.
- TV panel inspected against the installed TV Server: service running, MySQL `MpTvDb`, server name shown.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
