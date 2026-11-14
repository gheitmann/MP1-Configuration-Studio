# MP1 Configuration Studio 2.0.0-beta.15

**Status:** Fifteenth preview. Adds the remaining special areas, video/disc zoom groups, and a generic editor for XML sections that do not have a bespoke page yet. Unsigned preview. Not installed locally (no UAC run).

## New in beta.15

### Spezialbereiche

A new **Spezialbereiche** page with editors for:

- Thumbnails (`thumbnails`)
- File menu (`filemenu`)
- GUI notifications (`guiNotifier`)
- USBUIRT (`USBUIRT`)
- SerialUIR (`SerialUIR`)
- WinLIRC (`WINLIRC`)
- RedEye (`RedEye`)
- Screen selector (`screenselector`)
- MediaInfo (`MediaInfo`)
- Extensions metadata (`myextensions2`)
- Video/DVD/Blu-ray/TV post-processing (`filter*` entries)
- Music view templates (`mymusic` presentation entries)
- Video view entries (`start*`/`last*`)

Each entry is inferred automatically:

- `yes`/`no` → checkbox
- everything else → editable text field

### Zoom

- Video now has a **Bild & Zoom** group with default aspect ratio and all aspect-ratio allow flags (`movies`).
- BD / DVD now have separate **Blu-ray Bild & Zoom** and **DVD Bild & Zoom** groups (`bdplayer`/`dvdplayer`).
- Language is editable on the Interface page (`gui/language`).

### General

- Dynamic section editor for XML entries not yet given a hand-built page.
- Filter dropdowns on Codecs continue to use installed DirectShow filters and hide encoder/compressor entries.

## Validation

- Fixture checks: **220/220** on x64.
- Spezialbereiche inspected at 100% with live thumbnails/file-menu/notification values.
- Dynamic entries are read from the live configuration and written through the same safe `StudioConfiguration` layer.

## Still separate files

The following original pages edit files other than `MediaPortal.xml` and intentionally remain classic for now:

- Keyboard mapping (`keymap.xml`), Keys page
- Music/Video view definitions (`MusicViews.xml`, `VideoViews.xml`)
- Music DSP (`musicdsp.xml`)
- Plugin-specific configuration windows

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
