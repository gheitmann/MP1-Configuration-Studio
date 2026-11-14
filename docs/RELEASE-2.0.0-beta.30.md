# MP1 Configuration Studio 2.0.0-beta.30

**Status:** Thirtieth preview. Adds an automated native-page matrix across text scales and a verified plugin uninstall. Unsigned preview. Not installed locally (no UAC run).

## New in beta.30

### Automated page matrix

The diagnostics suite now builds **every native schema page at 100 %, 125 %, 150 %, and 175 % text scale**:

- 24 sections per scale
- form font size is asserted for each scale
- the run also covers the page editors (keymap, views, DSP, TV, plugins, raw XML, files, system, virtual drive)

This turns the previous manual 175 % screenshots into a repeatable regression check. It found no layout-construction exceptions.

### Plugin uninstall

`tools/Install-MPPlugin.ps1` now supports `-Uninstall`:

```powershell
.\tools\Install-MPPlugin.ps1 -Uninstall
```

- removes `MP1.ConfigurationStudio.Plugin.dll` from `plugins\Windows`
- removes `MP1StudioSettings.xml` from every skin
- sets the `MP1 Configuration Studio` entry in `MediaPortal.xml` to `no`
- creates a configuration backup before changing the entry
- leaves other plugin entries untouched

The script now also resolves the DLL and skin next to itself first, so the packaged plugin ZIP works without the repository layout.

## Validation

- Fixture checks: **234/234** on x64.
- New checks:
  - native pages build at 100 %, 125 %, 150 %, and 175 %
  - 24 sections per scale
- Plugin uninstall sandbox run:
  - removed the DLL
  - removed the skin file from 2 skins
  - changed only `MP1 Configuration Studio` to `no`, kept `Other=yes`
  - created `MediaPortal.xml.plugin-backup-*`
- Production configuration untouched.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
