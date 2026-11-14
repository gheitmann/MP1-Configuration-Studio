# MP1 Configuration Studio 2.0.0-beta.36

**Status:** Thirty-sixth preview. Full audit of the classic Configuration controls; every fixed-choice field is now a selection in both the desktop Studio and the MediaPortal plugin. Unsigned preview.

## Why the gap existed

The schema was derived from `MediaPortal.xml` keys and page structure, but the control types of the classic `Configuration.exe` were not compared field by field. Entries that the classic UI offers as a ComboBox or list were therefore editable as free text. That audit is now part of the implementation.

## Audit method

- Instantiated every classic `Configuration.Sections.*` class from the installed `Configuration.exe` (161 ComboBox controls).
- Read each control's items, labels, and configuration key from the MediaPortal 1.39 source.
- Added a shared `ChoiceCatalog` used by both UIs.
- Converted classic ComboBox fields and applied the catalog to automatically generated "More settings" rows.

## Selection catalog (desktop + plugin)

| Area | Field | Choices |
| --- | --- | --- |
| Blu-ray | Region | A, B, C |
| Blu-ray | Preferred audio codec | AC3, AC3+, DTS, DTS-HD, DTS-HD Master, LPCM, TrueHD |
| Blu-ray/DVD | Audio/subtitle language | installed culture names |
| DVD | Aspect ratio correction | Follow stream, Crop, Letterbox, Stretch |
| DVD | Display mode | Default, 16:9, 4:3 Pan Scan, 4:3 Letterbox |
| Subtitles | Engine | MPC-HC, DirectVobSub, FFDShow, XySubFilter, Disabled |
| Subtitles | Font | installed font families |
| Subtitles/movie | Language | ISO language codes |
| TV | Deinterlace | None, Bob, Weave, Best |
| TV | Episode info | None, Number, Title, Number + Title |
| Music files | Selection action | Play, Queue |
| Music files | Date for new files | Current, creation, last write |
| Audio player | Player | BASS, ASIO, WASAPI, internal DirectShow |
| Audio player | Resume by | Genre, Title, Filename, Album, Artist, Albumartist, Composer, Conductor |
| Audio player | Upmix mono/stereo/4.0/5.1 | classic speaker modes |
| Audio player | WASAPI speakers | Mono, Stereo, Quadrophonic, 5.1, 7.1 |
| Video database | Actors list fetch size | Short, Long |
| General | DX9 filtering mode | six classic filter modes |
| General | Start screen | installed monitors |
| Remote | HID profile | installed mapping profiles |
| Interface | Skin | installed skins |
| Interface | Language | installed language files |
| BD/DVD/Video | Aspect, layout, sort, autoplay | existing schema options |

All existing schema Combo fields keep their dropdown in the desktop and open the MediaPortal selection dialog in the plugin. The desktop also applies the catalog to fields that only appear in the automatic **More settings** group.

## Validation

- Fixture checks: **244/244** on x64.
- New catalog checks cover DVD aspect/display, Blu-ray region/codec, TV options, music options, audio player/upmix, subtitle engine, actor list.
- Desktop screenshots:
  - **BD / DVD**: Audio language, subtitle language, preferred audio codec, region code, and default aspect are dropdowns.
  - **Music**: Resume by, audio player, and all four upmix fields are dropdowns.
- Plugin selection dialogs already verified for skins and languages; the same shared catalog drives the new fields.

## Remaining hardware-dependent lists

Lists that depend on installed drivers, remote hardware, or loaded plugins (for example BASS sound devices, FireDTV device names, TV hostnames, idle-plugin window IDs) still accept the current value and remain editable because the classic UI enumerates them at runtime. They are documented rather than guessed.

## Install

The combined setup installs the plugin and skin. The standalone plugin ZIP remains available for manual install/uninstall.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
