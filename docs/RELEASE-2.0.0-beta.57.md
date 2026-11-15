# MP1 Configuration Studio 2.0.0-beta.57

**Status:** Fifty-seventh preview. Adds a right-click action menu to plugin tiles so configuration is reachable without scrolling back to the top. Unsigned preview.

## Plugin overview

- **Right-click any plugin tile** for quick actions:
  - Configure
  - Enable / disable
  - In Home
  - In Plugins
  - Browse and install plugins…
  - Classic configuration
- The menu uses the Studio light/dark palette and shows the current Home/Plugins membership.
- A tooltip and the overview status line explain the right-click gesture.
- Double-click still opens a configurable plugin directly.
- The top action bar remains available when the list is scrolled to the top; no scrolling is required for right-click actions.

## Validation

- Fixture checks: **276/276** on x64.
- Verified visually: context menu over a tile, selected plugin state, enabled/disabled entries and Home/Plugins checkmarks.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
