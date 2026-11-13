# MP1 Configuration Studio 2.0.0-beta.4

**Status:** Fourth preview of the new native Studio interface. The stable 1.9.0 interface remains the default. Unsigned preview.

## New in beta.4

### TV

- Extended the live-TV page with notification duration and beep, channel change by number, index sorting, and TsReader relaxation.
- New **Picture & sound** group: deinterlacing, default aspect ratio, and all aspect-ratio allow flags.
- New **Teletext** group: hide/transparent/remember and maximum font size.
- Aspect-ratio values use the exact strings saved by MediaPortal (`Normal`, `Original`, `Zoom`, `Zoom 14:9`, `Stretch`, `Non-linear Stretch`, `Letterbox 4:3`, `CinemaScope 2:35`).

### Radio

- Start group, ascending sorting, layout, sort order, and channel-number options are now editable.

### New pages

- **TV-Aufnahmen** – recording-list layout/sort and TV-guide minutes per block.
- **Untertitel** – engine, language, search paths, auto-load, save behaviour, font, size, colour, border, shadow, character set, relative position, style override, and delay step.

### Validation

- Fixture checks: **216/216** on x64.
- Every schema section is now constructed by the test harness, including the three media-folder editors, TV recordings, and subtitles.
- Live check: toggling `Teletext verbergen` on the TV page and saving changed **only** `mytv/teletextHidden`; all other entries stayed identical.

## Still classic

- Plugin configuration keeps the original windows.
- Very advanced areas such as dynamic refresh-rate profiles, renderer internals, and remote-control device settings are not rebuilt yet.
- The stable 1.9.0 interface remains the default; start the new UI with `--native` or Tools → New interface (beta).

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
