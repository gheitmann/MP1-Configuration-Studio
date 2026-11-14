# MP1 Configuration Studio plugin 2.0.0-beta.36

**Status:** Preview aligned with desktop Studio beta.36. Unsigned preview.

## New

- Shared `ChoiceCatalog` with the classic Configuration's fixed choices.
- Selection dialogs for Blu-ray region/codec, DVD aspect/display, subtitle engine, TV deinterlace/episode info, music play/queue and date, audio player/resume/upmix/WASAPI speakers, actor list size, DX9 filtering, start screen, HID profiles, skins, languages, and fonts.
- Integer choice fields also open a selection dialog instead of the numeric keyboard.
- Automatic "More settings" entries use the same catalog.

## Verified

- Desktop BD/DVD and Music pages render the new fields as dropdowns.
- Plugin selection dialogs for skins and languages; shared catalog wired into the plugin.
- 244/244 fixture checks; plugin smoke test passes (window 8917).

## Limits

- Hardware/driver/plugin-dependent lists (BASS sound devices, FireDTV names, TV hostnames, idle-plugin window IDs) remain editable.
- Unsigned; changes require a MediaPortal restart.
