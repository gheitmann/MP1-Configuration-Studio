# MP1 Configuration Studio 2.0.0-beta.59

**Status:** Fifty-ninth preview. Usability audit of the running program: the keyboard shortcuts documented in the help dialog now work in the native Studio, and the status badge no longer stays stuck after a reverted change. Unsigned preview.

## Usability audit

Walked the running program as a normal HTPC user: start page, navigation, search, Volume editing and save flow, pending-changes dialog, close prompt with Don't save, plugin overview with selection, enable/disable, context menu and scrolling, keyboard shortcuts, Raw XML access and status link.

Findings and fixes:

- **Keyboard shortcuts:** the help dialog documented shortcuts that did not work in the native interface. Implemented:
  - **F1** opens online help for the current section (wiki search for the section title).
  - **Shift+F1** opens the keyboard shortcuts overview.
  - **Alt+Left / Alt+Right** navigate back/forward.
  - **Ctrl+Plus / Ctrl+Minus** increase/decrease text and interface size.
  - The dialog now also lists **Ctrl+S** (save), **Ctrl+W** (close) and **Ctrl+Page Up/Down** (previous/next section).
- **Status badge:** toggling a plugin off and on again left "Unsaved changes" visible in an older build even after the pending change was reverted. The badge now re-evaluates the pending state after every change; when nothing is pending it shows Ready again.
- **Plugin overview status:** the status line now uses ellipsis so long hints do not clip on narrow windows.

## Verified scenarios

- Project start page, sidebar navigation and search filter with Enter to open the first result.
- Volume page: change, save with backup, revert, pending dialog shows the exact entry.
- Plugins: tile selection, enable/disable with pending state, right-click action menu, dismiss without selection (no error), page scrolling with synchronized scrollbar.
- Close prompt with Save/Don't save/Cancel; empty prompt suppressed.
- Keyboard shortcuts dialog opens from the toolbar and via Shift+F1.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
