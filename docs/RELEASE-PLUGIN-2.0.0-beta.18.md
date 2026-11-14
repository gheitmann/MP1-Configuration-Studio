# MP1 Configuration Studio plugin 2.0.0-beta.18

**Status:** Preview aligned with the desktop Studio beta.18 schema. Unsigned preview; test on a backed-up configuration.

## What changed

- Version aligned with desktop beta.18.
- No plugin UI changes since beta.17.
- Desktop-only pages remain hidden in MediaPortal: XML files, raw XML, keyboard, views, DSP, and TV Server administration.
- TV Server counts and SetupTv access are desktop-Studio features.

## Install

1. Unpack this ZIP.
2. Run `Install-MPPlugin.ps1` in an elevated PowerShell.
3. Enable **MP1 Configuration Studio** in MediaPortal Configuration → Plugins.
4. Start MediaPortal and open the plugin.

## Limits

- Folder/share editors, XML files, keyboard mapping, views, DSP, and TV Server administration remain desktop-Studio or SetupTv features.
- The plugin was built and its `ISetupForm` smoke test passed (window 8917).
- Unsigned.
