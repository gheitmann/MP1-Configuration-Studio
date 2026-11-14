# MP1 Configuration Studio 2.0.0-beta.9

**Status:** Ninth preview of the new native Studio interface and the first beta where it is the **default** entry point. The 1.9 shell and the original MediaPortal configurator remain available. Unsigned preview.

## Default interface

- Starting `MP1ConfigurationStudio.exe` without arguments now opens the **native Studio interface**.
- `--native` remains available as an explicit switch.
- `--studio` starts the previous 1.9 Studio shell.
- `--classic` starts the original MediaPortal `Configuration.exe`.
- `--section=<id>` still opens a native page directly, for example `--section=plugins`.

## Included from beta.6–beta.8

- Plugin activation for window plugins, plugin DLLs, and process plugins, read dynamically from `MediaPortal.xml`.
- Fernbedienung page for HCW, X10, HID, AppCommand, IRTrans, FireDTV, and Centarea.
- Extended TV-Server fields and preferred audio/subtitle languages.
- Manual **Sicherung** button with ZIP backup.
- Folder pickers for recording, timeshift, and playlist folders.
- Keyboard: `Ctrl+S`, `Ctrl+W`, `Ctrl+PageUp/PageDown`.
- Live settings search in the header with match count and Enter/Esc handling.

## Validation

- Fixture checks: **220/220** on x64, including unique section IDs, German/English labels, combo options, and construction of every schema section.
- No-argument start verified: opens **MP1 Configuration Studio · Neue Oberfläche**.
- `--studio` verified: opens **MP1 Configuration Studio · MediaPortal 1.39**.
- Production `MediaPortal.xml` was not modified.

## Still classic

- Plugin configuration windows.
- Sorting/layout internals for music and video views.
- The 1.9 Studio shell remains available with `--studio`; the original configurator with `--classic`.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe` — the native interface opens by default.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
