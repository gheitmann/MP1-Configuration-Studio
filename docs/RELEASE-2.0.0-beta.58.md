# MP1 Configuration Studio 2.0.0-beta.58

**Status:** Fifty-eighth preview. Fixes the error dialog that appeared after dismissing a plugin context menu without choosing an action. Unsigned preview.

## Plugin context menu

- The menu is now reused instead of being disposed inside its own `Closed` event.
- Dismissing the menu with a left-click elsewhere no longer accesses a disposed `ContextMenuStrip` and no longer opens the global error dialog.
- Old menu items are cleared and disposed safely before the menu is rebuilt.
- Configure, Enable/Disable, Home/Plugins, Browse and Classic actions are unchanged.

## Validation

- Fixture checks: **276/276** on x64.
- Reproduced the previous sequence (right-click, no selection, left-click outside); the Studio stays open without an error dialog.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
