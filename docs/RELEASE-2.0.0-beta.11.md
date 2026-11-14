# MP1 Configuration Studio 2.0.0-beta.11

**Status:** Eleventh preview. The native interface is the default; the 1.9 shell is available from the sidebar. Unsigned preview.

## New in beta.11

### Music view

- New **Ansicht** group on the Music page:
  - layout (`Liste`, small/large icons, filmstrip, album view, playlist, CoverFlow),
  - sort order with all 16 `MusicSort.SortMethod` values,
  - ascending sorting.

### Video view

- New **Ansicht** group on the Video page:
  - layout with all seven MediaPortal facade layouts,
  - sort order and root-folder sort order with all 11 `VideoSort.SortMethod` values,
  - ascending sorting.

The combo values use the exact integers MediaPortal stores in `mymusic` and `myvideo`.

## Included from beta.10

- **Studio 1.9-Oberfläche** entry in the native sidebar, with clean single-instance hand-off.
- Native interface remains the default; `--studio` starts the 1.9 shell and `--classic` the original configurator.

## Validation

- Fixture checks: **220/220** on x64.
- Music view inspected at 125%: layout `Liste`, sort `Name`, ascending enabled.
- Video view inspected at 125%: layout `Liste`, sort/root sort `Name`, ascending enabled.
- Production `MediaPortal.xml` was not modified.

## Still classic

- Plugin configuration windows.
- Music view templates (column definitions) and other advanced list customisation.
- TV dynamic refresh-rate profiles and remote-device-specific extras remain on their existing pages.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe` — the native interface opens by default.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
