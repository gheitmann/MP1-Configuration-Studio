# MP1 Configuration Studio plugin 2.0.0-beta.17

**Status:** Preview aligned with the desktop Studio beta.17 schema. Unsigned preview; test on a backed-up configuration.

## What it is

A window plugin that brings the Studio schema into MediaPortal itself. It reads and writes the live `MediaPortal.xml` through MediaPortal's `MPSettings` cache and creates a ZIP backup before saving.

## What changed in beta.17

- Schema updated to beta.17 (keyboard, views, DSP, TV Server status fields).
- Desktop-only pages **Spezialdateien**, **XML-Rohdaten**, **Tastatur**, **Musik-Ansichten**, **Video-Ansichten**, and **Musik-DSP** are hidden inside MediaPortal.
- TV Server status rows point to the desktop Studio.
- Window ID, compatibility attributes, and skin are unchanged from beta.13.

## Coverage

- All `MediaPortal.xml` schema pages except the desktop-only file/XML and structured-file pages.
- Boolean, combo, integer, text, and folder rows.
- Numeric and virtual keyboards for integer/text editing.
- Read-only rows for media-folder lists, dynamic entries, plugin lists, and info entries, pointing to the desktop Studio.

## Install

1. Unpack this ZIP.
2. Run `Install-MPPlugin.ps1` in an elevated PowerShell (or copy the DLL to `plugins\Windows` and the XML to every skin folder).
3. Enable **MP1 Configuration Studio** in MediaPortal Configuration → Plugins, then choose **In Home** or **In Plugins**.
4. Start MediaPortal and open the plugin. Save with **Sichern**.

The skin file targets 1920×1080 skins and uses common list textures. For other skin resolutions, scale the positions.

## Limits

- Folder/share editors, XML files, raw XML, keyboard mapping, views, DSP, and TV Server administration remain desktop-Studio or SetupTv features.
- Some values are cached by MediaPortal components and need a restart.
- The plugin was built and its `ISetupForm` smoke test passed (window 8917).
- Unsigned.
