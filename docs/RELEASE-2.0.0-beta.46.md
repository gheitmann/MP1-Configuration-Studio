# MP1 Configuration Studio 2.0.0-beta.46

**Status:** Forty-sixth preview. Refines the sidebar scrollbar, Standard/Expert modes, the close prompt, About window and raw XML safety. Unsigned preview.

## Sidebar scrolling

- Wheel scrolling now updates the thumb immediately (`StudioScrollHost.SyncNow` after the wheel filter).
- Arrow keys move the focus through the sidebar entries; the list scrolls to keep the focused entry visible and the thumb follows.
- After startup the selected navigation entry receives focus, so keyboard navigation works without a prior click.
- Reaching the bottom shows the complete **Studio 1.9 interface** entry again.

## Standard vs Expert

Standard mode is now curated for the first ten starts and shows only the everyday pages:

General, Volume, Pictures, Interface, Start menu, Video, Music, Media folders, TV, Radio, TV recordings, Subtitles, BD/DVD.

Expert adds:

System and drives, Video database, Video views, Music files, Music views, Music DSP, Keyboard, Codecs, Plugins, Plugin data, Special areas, Special files, Raw XML, Remote.

The automatic **More settings** tab is also hidden in Standard. Switching from an advanced page to Standard navigates to the first visible page.

## Closing and unsaved changes

- Closing (or switching to classic/1.9, language, theme) with unsaved work opens the pending-changes list instead of a Windows Yes/No box.
- The list offers **Save**, **Don't save** and **Cancel**; Save writes configuration and XML editors first and only closes when everything was saved.
- **Unsaved changes** in the footer is underlined, uses the accent colour, shows a hand cursor and brightens on hover.

## About and icons

- Tools → About now opens a themed Studio dialog (logo, version, licence, MediaPortal note) instead of a native MessageBox.
- The toolbar help icon is now a keyboard icon and opens the keyboard-shortcut window.

## Raw XML

- The `×` button removes the entry from `MediaPortal.xml` – it is not a reset to defaults.
- Every remove asks for confirmation and states that saving creates a ZIP backup; the button has a tooltip and the page shows a short explanation line.

## Validation

- Fixture checks: **259/259** on x64 (themed About dialog added).
- Verified: wheel/keyboard sidebar scrolling with moving thumb, Standard page set and hidden More settings, close dialog with Save/Don't save/Cancel, underlined Unsaved changes, About dialog construction.
- Release workflow remains green.

## Install

The combined setup installs application and optional plugin together. No configuration format changes.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
