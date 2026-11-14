# MP1 Configuration Studio 2.0.0-beta.32

**Status:** Thirty-second preview. Bundles the MediaPortal plugin into the setup as an optional component. Unsigned preview. Not installed locally (no UAC run).

## New in beta.32

### One setup for app and plugin

The Inno Setup installer now offers two components:

- **MP1 Configuration Studio (desktop)** — fixed, always installed
- **MediaPortal plugin (optional)** — selected by the `full` setup type

When the plugin component is selected:

- `MP1.ConfigurationStudio.Plugin.dll` is installed into `{app}\plugins\Windows`
- `MP1StudioSettings.xml` is installed next to the DLL and copied into every existing skin under `C:\ProgramData\Team MediaPortal\MediaPortal\skin`
- the uninstaller removes the plugin DLL and the per-skin copies, but leaves unrelated skin files alone

### MediaPortal detection

The setup keeps using the MediaPortal uninstall registry keys (64-bit and 32-bit) with Program Files fallbacks, and refuses to continue when `Configuration.exe` is missing.

## Validation

- Fixture checks: **238/238** on x64.
- A sandbox build of the same script (`-DTestInstall=1`, lowest privileges, `%LOCALAPPDATA%` target) was installed and uninstalled silently:
  - full install: app files, `plugins\Windows\MP1.ConfigurationStudio.Plugin.dll`, `MP1StudioSettings.xml`, and skin copies in `SkinA`/`SkinB`
  - uninstall: app folder, plugin DLL, and skin copies removed; unrelated `keep.txt` files preserved
  - components=`application` only: app installed, no plugin DLL, no skin copies
- The release setup was compiled from the same script without test defines.
- Production configuration untouched.

## Install

Run the 2.0 setup as administrator and leave the plugin component selected to install both parts. The portable ZIP remains app-only; the plugin ZIP remains available for manual install/uninstall.

The elevated installation itself and the MediaPortal skin test still require user interaction.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
