# MP1 Configuration Studio 2.0.0-beta.3

**Status:** Third preview of the new native Studio interface. The stable 1.9.0 interface remains the default. Unsigned preview.

## New in beta.3

### Medienordner (media folders)

- New native page for video, music, and picture folders.
- Add, edit, and remove folders with a folder picker and optional scan toggle.
- Existing share entries such as pincode, server, login, password, port, remote path, view, Wake-on-LAN, and host detection are preserved when folders are edited, reordered, or removed.
- The share editor now reports unsaved changes to the main window, and removing a folder cleans up all of its numbered entries.

### Additional core pages

- **TV** – TV server host, Wake-on-LAN, recording/timeshift folders, AC3 preference, and live-TV behaviour.
- **Radio** – channel list behaviour, sorting, layout, and auto-start.
- **Codecs** – video, audio, and TV decoder/renderer fields.
- **BD / DVD** – internal/external player, paths, languages, subtitles, and region.
- **Plugins** – explains that plugin configuration keeps its original windows; the classic interface remains one click away.

### Layout and usability

- The sidebar scrolls with the mouse wheel and keeps its position when switching pages; no unexpected jump when a page is selected.
- Dark scrollbars and a dark window title bar match the Studio palette.
- The 100–175% size selector now reflows the header, sidebar, content columns, and footer correctly at every step.
- The media-folder editor sizes itself to its rows so its action buttons stay reachable at 150% and 175%.
- Dark-themed media-folder list with a custom header instead of the light system header.

## Validation

- Fixture checks: 215/215 on x64.
- Manual native verification on the isolated MediaPortal 1.39 x64 runtime:
  - all sidebar pages opened and rendered,
  - 100%, 125%, and 175% layouts reflowed and stayed usable,
  - folder add, edit, remove, and reload,
  - save with ZIP backup and atomic replacement,
  - configuration diff confirmed only the intended entries changed and all unknown share fields survived.

## Known limitations

- TV, Radio, codecs, and BD/DVD are schema-based previews; some values are still free-text rather than filtered pickers.
- Plugin configuration intentionally stays on the classic/original windows.
- The stable 1.9.0 shell remains the default; start the new UI with `--native` or Tools → New interface (beta).
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, clean-VM checks, and SignPath approval remain pending.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe` and choose Tools → New interface (beta), or pass `--native`.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
