# MP1 Configuration Studio 2.0.0-beta.6

**Status:** Sixth preview of the new native Studio interface. The stable 1.9.0 interface remains the default. Unsigned preview. Portable build; no UAC-driven installation was performed for this preview.

## New in beta.6

### Plugins

- The Plugins page can now enable and disable window plugins, plugin DLLs, and process plugins directly from `MediaPortal.xml`.
- The lists are read dynamically from the configuration, so newly installed or unknown plugins appear automatically and are preserved on save.
- Plugin configuration windows intentionally remain on the classic page; the page explains this and warns that activation changes require a restart.

### TV-Server

- Added automatic MAC address, MAC address, preferred audio/subtitle languages, DVB bitmap/teletext subtitles, closed captions, subtitle auto-show, dual-mono audio, and RTSP.

### Fernbedienung

- New page for Hauppauge HCW, X10, HID, AppCommand, IRTrans, FireDTV, and Centarea receivers, including ports, repeat timing, profiles, and verbose logging.

### Musik

- Playlist folder is now editable.

### Diagnostics

- New `--section=<id>` switch opens the native interface directly on a page, for example `--section=plugins` or `--section=remote`.
- Deep links scroll the sidebar to the selected page.

## Validation

- Fixture checks: **216/216** on x64, with every schema section constructed.
- Dynamic Startmenü editing was verified end-to-end: toggling one entry changed only that entry.
- Remote settings verified: toggling a HCW log option changed only `remote/HCWVerboseLog`.
- The Plugins page uses the same verified dynamic toggle path; its lists were inspected against the live configuration.

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
