# MP1 Configuration Studio – MediaPortal plugin

This plugin makes the Studio schema available **inside MediaPortal**. It is a window plugin (`GUIWindow`, `ISetupForm`) that reads and writes the current `MediaPortal.xml` through MediaPortal's own `MPSettings` cache.

## What it shows

- Every schema section from `NativeSettingsSchema` (General, Interface, Start menu, Pictures, Video, Video database, Music, Music files, Media folders, TV, Radio, TV recordings, Subtitles, Remote, Codecs, BD/DVD, Plugins).
- Boolean values toggle with OK/left/right.
- Combo values cycle with OK/left/right.
- Integer values open the MediaPortal numeric keyboard; text and folder values open the virtual keyboard.
- Media-folder lists, dynamic plugin lists, and info rows are visible but point back to the desktop Studio, because they need the dedicated editors.

All settings are read from and written to the live configuration. **Sichern** creates a ZIP backup in `%LOCALAPPDATA%\MP1 Configuration Studio\backups` and then calls `Settings.SaveCache()`. Some changes only take effect after restarting MediaPortal.

## Build

The project references the installed MediaPortal 1.39 assemblies. Pass the MediaPortal folder if it is not in the default location:

```powershell
dotnet build plugin\MP1.ConfigurationStudio.Plugin.csproj -c Release -o dist\plugin `
  -p:MediaPortalDir="C:\Program Files\Team MediaPortal\MediaPortal"
```

The plugin assembly is versioned `1.39.0.0` for MediaPortal's compatibility check and carries:

```csharp
[assembly: CompatibleVersion("1.39.0.0", "1.39.0.0")]
[assembly: UsesSubsystem("MP")]
```

## Install

Run an elevated PowerShell:

```powershell
.\tools\Install-MPPlugin.ps1 -Enable
```

The script copies the DLL to `plugins\Windows`, copies `MP1StudioSettings.xml` into every skin under `C:\ProgramData\Team MediaPortal\MediaPortal\skin`, and optionally enables `MP1 Configuration Studio` in `MediaPortal.xml` after creating a backup.

Then enable the plugin in **MediaPortal Configuration → Plugins** and choose **In Home** or **In Plugins**. The plugin opens as **MP1 Configuration Studio**.

## Skin

`plugin\Skin\MP1StudioSettings.xml` is written for 1920×1080 skins (Backdrop, Titan, DefaultWideHD, MayaHD, Apollo, PureVisionHD) and uses the common `list_background_focus_ledon.png` / `list_background_nofocus_ledon.png` textures. For other resolutions, scale the positions in the XML.

## Current limits

- Folder/share editors and dynamic plugin lists are shown read-only in the plugin.
- MP's own Settings/Skin engine cannot host arbitrary WinForms controls, so this is a dedicated MP window, not an embedded Studio window.
- Text input depends on the MediaPortal virtual/numeric keyboard skin.
- The plugin writes through MP's settings cache; a few values are cached by MP components and need a restart.

## Smoke test

`plugin\Smoke` instantiates the plugin and checks `ISetupForm` identity:

```powershell
dotnet run --project plugin\Smoke\PluginSmoke.csproj -c Release
```
