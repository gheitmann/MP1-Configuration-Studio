# MP1 Configuration Studio 2.0.0-beta.7

**Status:** Seventh preview of the new native Studio interface. The stable 1.9.0 interface remains the default. Unsigned preview. No UAC-driven installation was performed for this preview.

## New in beta.7

### Manual backup

- New **Sicherung** button in the footer creates a ZIP backup of the current `MediaPortal.xml` at any time, independent of saving.
- The status line shows the created file name; backups are stored in the local Studio backup folder.

### Folder pickers

- Recording folder, timeshift folder, and playlist folder now have a **…** browse button.
- Matching folders are pre-selected in the picker.

### Keyboard

- **Ctrl+S** saves the configuration.
- **Ctrl+W** closes the window (the unsaved-changes guard still applies).
- **Ctrl+PageDown / Ctrl+PageUp** move to the next or previous page and keep the sidebar selection visible.

## Validation

- Fixture checks: **216/216** on x64.
- TV page inspected at 125%: folder browse buttons render next to the path fields.
- Manual backup verified: clicking **Sicherung** created `native-manual-20261114-014738-090.zip` (10.5 KB) and updated the status line.
- Production configuration was not modified.

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
