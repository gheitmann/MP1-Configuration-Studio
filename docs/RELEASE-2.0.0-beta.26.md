# MP1 Configuration Studio 2.0.0-beta.26

**Status:** Twenty-sixth preview. Adds the remaining non-XML/system page: autoplay, CD/DVD speed, and system checks. Unsigned preview. Not installed locally (no UAC run).

## New in beta.26

### System und Laufwerke

New native page with:

- **Autoplay** options for audio, video, and photos:
  - Ask
  - Play
  - Do not play
- **CD/DVD speed** fields for:
  - `cdspeed/drivespeedCD`
  - `cdspeed/drivespeedDVD`
  - `cdspeed/disableCD`
  - `cdspeed/disableDVD`
- **System checks**:
  - Detects Windows Media Center `ehRecvr`/`ehSched` services.
  - Shows whether the Media Center policy is active.
  - **MPTray with Windows** checkbox writes/removes the per-user `Run` entry.
  - Button to open the classic checks for administrator-level policy changes.

### Virtual drive

The virtual-drive fields were already on the BD/DVD page (`daemon` section): enabled, program path, extensions, drive number, and ask-before-playing.

## Validation

- Fixture checks: **224/224** on x64.
- CD/DVD speed round-trip: set `cdspeed/drivespeedCD=4,8,16`, saved, verified in `MediaPortal.xml`.
- Autoplay values are written through the same `general/autoplay_*` keys as the classic page (`Ask` / `Yes` / `No`).
- System checks page inspected; Media Center status and MPTray registry state are read.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
