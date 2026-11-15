# MP1 Configuration Studio 2.0.0-beta.52

**Status:** Fifty-second preview. Adds the classic plugin icon overview, lists only installed decoder filters for codecs, and removes the retired IRC link. Unsigned preview.

## Plugin overview

The Plugins page now opens with a classic-style icon overview:

- **Window plugins**, **external players** and **process plugins** are shown in separate groups.
- Icons are read from the plugin assemblies; built-in entries use themed fallback icons for home, music, pictures, video, TV, radio, settings, weather, news and games.
- Status dots show whether a plugin is enabled.
- Selecting a tile exposes **Configure**, **Enable / disable**, **In Home**, **In Plugins**, **Refresh**, **Browse and install plugins…** and **Classic configuration**.
- State changes write the classic keys (`plugins`, `pluginsdlls`, `pluginswindows`, `home`, `myplugins`) and appear in the normal pending-changes flow.
- The overview is cached for 12 hours; **Refresh** forces a fresh scan after installing or removing plugins.

The existing Plugin configuration, Enabled, Home, My Plugins and DLL tabs remain available.

## Codecs and Renderer

The codec lists now contain only **installed decoder filters** registered on this system. Encoders, compressors, muxers, parsers, splitters, source filters, renderers and DirectShow utilities are removed from the decoder lists. Audio endpoint devices such as “Realtek HD Audio output” and MediaPortal AudioSwitcher no longer appear as audio codecs. Splitter and source lists still show only installed filters.

## Project page

- The retired IRC entry has been removed from the Project page.
- Homepage, forum, documentation, GitHub and donation links remain.

## Validation

- Fixture checks: **276/276** on x64.
- Verified: plugin icon grid, selection and action bar, enabled dots, cached scan; codec list filtered to installed decoders (video and audio) by runtime inspection.
- Production `MediaPortal.xml` was not modified.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal logo artwork is used for project identification only.
