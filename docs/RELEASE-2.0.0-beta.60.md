# MP1 Configuration Studio 2.0.0-beta.60

**Status:** Sixtieth preview. Pending changes dialog (and every other dialog) now uses the official MediaPortal icon and the generated application icon instead of the old artwork. Unsigned preview.

## Dialogs

- All dialogs and the main window now use the official MediaPortal icon from team-mediaportal.com as their window icon. The old colourful mascot icon is gone from title bars.
- The application, setup and taskbar icons are regenerated from the same official artwork (`assets\MediaPortal.ico`).
- The Pending Changes dialog was verified in dark and light theme: dark body, dark title bar with the new icon; light body with the new icon.
- `StudioDialog.Apply` now assigns the Studio icon to every dialog unconditionally, so no dialog can fall back to the classic icon.

## Validation

- Fixture checks: **276/276** on x64.
- Verified visually: Pending Changes dialog title bar icon and theming in dark and light theme; main window and dialog icons match.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal artwork is used for identification only.
