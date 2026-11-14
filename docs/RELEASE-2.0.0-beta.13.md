# MP1 Configuration Studio 2.0.0-beta.13

**Status:** Thirteenth preview. Core configuration pages are complete; this beta adds the remaining built-in settings and a first pass on the visual refresh. Unsigned preview.

## Remaining pages and settings

- **Musik-Dateien** now includes MP3 import, playlist layout/sort/order, and the remaining music playback switches.
- **TV** and **Radio** gained search mode and sort settings.
- **Oberfläche** gained the skin name.
- **Video** gained the remaining player settings from beta.12.
- **BD / DVD** gained the virtual-drive (Daemon Tools) settings.
- **Video-Datenbank** and **Musik-Dateien** complete the media database/import coverage.
- Built-in configuration sections are now covered. Plugin-owned windows, the Extensions installer, Screen Selector, the file menu, and notifier metadata intentionally remain on the classic/original path.

## Visual refresh (test)

- New **Glas** toggle in the header, persisted in `appearance.xml`.
- Glass-style gradient surfaces for header, sidebar, and footer, with a top highlight and border.
- Accent bars on group headings.
- Rounded, hover-aware buttons for Backup, Save, Close, and the glass toggle.
- Transparent header labels so the gradient remains visible.

## Workflow

- Sidebar order continues to follow the familiar MediaPortal sequence: General → interface/media → folders → TV/Radio → recordings/subtitles/remote → codecs/discs/plugins.
- Page layouts keep the same top-to-bottom grouping as the original configuration pages.

## Validation

- Fixture checks: **220/220** on x64.
- All new pages inspected at 125 % with live values.
- Glass on/off inspected; the toggle persists through `appearance.xml`.
- Production `MediaPortal.xml` was not modified.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe` — the native interface opens by default.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
