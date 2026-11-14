# MP1 Configuration Studio plugin 2.0.0-beta.13

**Status:** First preview of the MediaPortal 1.39 window plugin. Unsigned preview; test on a backed-up configuration.

## What it is

A window plugin that brings the Studio schema into MediaPortal itself. It reads and writes the live `MediaPortal.xml` through MediaPortal's `MPSettings` cache and creates a ZIP backup before saving.

## Coverage

- Every schema section: General, Interface, Start menu, Pictures, Video, Video database, Music, Music files, Media folders, TV, Radio, TV recordings, Subtitles, Remote, Codecs, BD/DVD, Plugins.
- Boolean, combo, integer, text, and folder rows.
- Numeric and virtual keyboards for integer/text editing.
- Read-only rows for media-folder lists, dynamic plugin lists, and info entries, pointing to the desktop Studio.

## Install

1. Build or unpack this ZIP.
2. Run `Install-MPPlugin.ps1` in an elevated PowerShell (or copy the DLL to `plugins\Windows` and the XML to every skin folder).
3. Enable **MP1 Configuration Studio** in MediaPortal Configuration → Plugins, then choose **In Home** or **In Plugins**.
4. Start MediaPortal and open the plugin. Save with **Sichern**.

The skin file targets 1920×1080 skins and uses common list textures. For other skin resolutions, scale the positions.

## Limits

- Folder/share editors and dynamic plugin toggles remain desktop-Studio features for now.
- Some values are cached by MediaPortal components and need a restart.
- The plugin was built and loaded by MediaPortal 1.39 without compatibility errors; full visual testing in every skin is still pending.
- Unsigned.
