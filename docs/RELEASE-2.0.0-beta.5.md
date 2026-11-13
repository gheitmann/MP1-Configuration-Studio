# MP1 Configuration Studio 2.0.0-beta.5

**Status:** Fifth preview of the new native Studio interface. The stable 1.9.0 interface remains the default. Unsigned preview.

## New in beta.5

### Startmenü

- New page that reads the MediaPortal home-menu entries directly from the configuration.
- Show or hide built-in entries and plugin-provided menu items.
- Unknown or newly installed plugin entries are listed automatically and preserved on save.

### Allgemein

- New **Vollbild & Start** group: fullscreen splash, resolution changes, delayed resume, monitor off, last module, audio-removal handling, RDP autostart, primary screen, and the FCU black-screen fix.
- New **Renderer & Bild** group: EVR, madVR, madVR for TV/3D, low-latency mode, internal DRC, VRM9 web streams, DX9 options, non-square pixels, exclusive mode, and DX9 filter mode.
- New **Dynamic Refresh Rate** group: automatic refresh-rate changes, notification, default Hz, device reset, forced refresh rate, and default profile.
- Host detection and the idle plugin are now editable.

### Musik

- New **Ausgabe & ReplayGain** group: ReplayGain, album ReplayGain, stream output level, skip steps, sound device, and WASAPI speaker count.

### Oberfläche

- Remember-last-focus, file-exists cache, adding video files to the database, and the My prefix.

## Validation

- Fixture checks: **216/216** on x64, including construction of every schema section.
- Live check: toggling `Wikipedia` on the Startmenü page and saving changed **only** `home/Wikipedia`; all other menu entries stayed identical.

## Still classic

- Plugin configuration keeps the original windows.
- Dynamic refresh-rate profiles, remote-control device settings, and other advanced areas remain on the classic pages for now.
- The stable 1.9.0 interface remains the default; start the new UI with `--native` or Tools → New interface (beta).

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
