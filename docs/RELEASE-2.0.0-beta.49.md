# MP1 Configuration Studio 2.0.0-beta.49

**Status:** Forty-ninth preview. Themes all custom dialogs, removes the duplicate shortcuts entry, adds a one-time Expert hint, suppresses empty close prompts, and aligns page order and Volume with the classic configurator. Unsigned preview.

## Dialogs

- **Keyboard shortcuts**, **Backup manager**, **Error**, **First run**, **Pending changes**, **About** and the new **Expert hint** now share the Studio palette, rounded buttons, window icon and a theme-aware title bar.
- No native white dialogs remain for Studio-owned windows. System file pickers and message boxes stay native by design.

## Toolbar and start

- Keyboard shortcuts was removed from the Tools menu; the keyboard icon in the toolbar opens it.
- On first start, a one-time themed hint explains Standard and Expert mode and offers **Show Expert mode**. The flag is stored in `appearance.xml`.
- The window paints its dark background before the first frame and uses double buffering, reducing the white flash at startup.

## Close prompt

- The Save/Don't save/Cancel list only appears when configuration entries or XML editors actually contain unsaved work. Closing with zero pending changes no longer shows an empty dialog.

## Order and Volume

- Sidebar order now follows the classic configurator: General, Volume, Interface, Start menu, Video, Video database, Video views, Subtitles, BD/DVD, TV, Radio, TV recordings, Music, Music files, Music views, Music DSP, Pictures, Media folders, Remote, Keyboard, Codecs, Plugins, Plugin data, System, Special areas, Special files, Raw XML.
- Volume now includes the classic **Control** (Wave/Master, handler scale and custom table), **Startup** (last known/system/custom plus value) and **On-screen display** sections.

## Validation

- Fixture checks: **263/263** on x64 (classic order, Volume fields, defaults, themed dialogs).
- Verified: one-time hint, themed Keyboard shortcuts in light theme, Volume tabs and values, sidebar order.
- Release workflow remains green.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
