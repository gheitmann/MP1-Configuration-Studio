# MP1 Configuration Studio 2.0.0-beta.16

**Status:** Sixteenth preview. Completes the XML side of the native configuration, fixes the DirectShow filter list, and opens real filter property pages. Unsigned preview. Not installed locally (no UAC run).

## New in beta.16

### DirectShow filter configuration

- The codec and audio-renderer lists now come from MediaPortal's own `FilterHelper` (`Core.dll`): 26 video decoders, 28 audio decoders, and 11 audio renderers on the reference system.
- Fallback enumeration uses DirectShowLib when `Core.dll` is unavailable.
- The **…** button next to every codec/renderer field opens the filter's real property page (`ISpecifyPropertyPages` + `OleCreatePropertyFrame`).
- Example verified: **LAV Video Decoder** opens its full Settings/Formats dialog.
- Filters are resolved by friendly name, so the value saved in `MediaPortal.xml` is unchanged.

### XML file editor

The **Spezialdateien** page now edits:

- `keymap.xml`
- `MusicViews.xml`
- `VideoViews.xml`
- `musicdsp.xml`
- `MediaPortalDirs.xml`
- `Gentle.config`, `OnlineVideoSites.xml`, `LatestMediaHandler.xml`, `InfoService.xml`, `SubCentral.xml`, `LogoManager.config`, `BackdropMenuEditor.xml`, `wikipedia.xml`, `FanartHandler.xml`, `IncompatiblePlugins.xml`, and `ScreenCalibration*.xml` when present
- Any other XML/configuration file via **Andere Datei …**

Each save:

- validates the XML before writing,
- creates a ZIP backup under `%LOCALAPPDATA%\MP1 Configuration Studio\backups`,
- preserves a UTF-8 BOM when the source had one,
- replaces the file atomically.

### Raw XML for every MediaPortal.xml section

- **XML-Rohdaten** lists **all** sections of `MediaPortal.xml`, including plugin sections such as `Trakt`, `onlinevideos`, `pluginSorting`, and `pluginswindows`.
- Values are inferred as checkboxes (`yes`/`no`) or text fields.
- A section filter makes large configurations manageable.
- Unknown values and sections remain untouched.

### Plugins

- The **Plugins** page now has a direct **Klassische Plugin-Konfiguration öffnen** button. Plugin windows still use their original classic UI, but no longer require searching for the entry in the sidebar.

## Coverage audit

All XML-based configuration is now reachable natively:

| Area | Where |
| --- | --- |
| `MediaPortal.xml` core settings | Bespoke Studio pages |
| `MediaPortal.xml` plugin/unknown sections | **XML-Rohdaten** |
| Key mapping | **Spezialdateien** → `keymap.xml` |
| Music/video views | **Spezialdateien** → `MusicViews.xml`, `VideoViews.xml` |
| Music DSP | **Spezialdateien** → `musicdsp.xml` |
| Other XML/config files | **Spezialdateien** → file list or **Andere Datei …** |
| DirectShow filter settings | **Codecs** → **…** |
| Plugin configuration windows | **Plugins** → classic configurator |
| TV Server channels/epg/database | Separate `SetupTv.exe` / TV Server database, not XML |

## Validation

- Fixture checks: **220/220** on x64.
- XML editor save verified against a test configuration: file content updated, ZIP backup created.
- Raw XML page inspected with the `general` section and section filter.
- Filter property page verified for LAV Video Decoder.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
