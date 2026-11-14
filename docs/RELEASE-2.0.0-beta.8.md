# MP1 Configuration Studio 2.0.0-beta.8

**Status:** Eighth preview of the new native Studio interface. The stable 1.9.0 interface remains the default. Unsigned preview. No UAC-driven installation was performed for this preview.

## New in beta.8

### Search

- New search box in the header with a cue banner (“Einstellungen durchsuchen…”).
- Filters the sidebar live across page titles, subtitles, group titles, field labels, and help text.
- The status bar shows how many pages match; **Enter** opens the first match, **Esc** clears the search.
- The classic/plugin entry remains visible while searching.

### Validation

- Fixture checks: **216/216** on x64.
- Live search verified with “Teletext”: the sidebar reduced to **TV** and the status showed **Suche: 1 Seite**.

## Included from beta.7

- Manual **Sicherung** button with ZIP backup and status line.
- Folder browse buttons for recording, timeshift, and playlist folders.
- **Ctrl+S**, **Ctrl+W**, **Ctrl+PageUp/PageDown** shortcuts.

## Still classic

- Plugin configuration windows.
- Sorting/layout internals for music and video views.
- The stable 1.9.0 interface remains the default; start the new UI with `--native` or Tools → New interface (beta).

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
