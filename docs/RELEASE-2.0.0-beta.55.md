# MP1 Configuration Studio 2.0.0-beta.55

**Status:** Fifty-fifth preview. Fixes plugin tile selection, removes the nested plugin scrollbar, speeds up tile clicks and drops the redundant Classic configuration tab. Unsigned preview.

## Plugin overview

- Only **one tile** can be selected at a time.
- The selection highlight is redrawn immediately and stays visible when the mouse leaves the tile.
- Tiles are no longer focusable controls, so clicking one no longer triggers scroll-to-control and long repaint delays.
- Selection repaints only the previously selected and newly selected tiles instead of the whole grid.
- Action-button labels are updated without re-laying out the entire action bar.

## Scrolling

- The plugin overview no longer has its own nested scrollbar.
- The overview grows to its content height and uses the page scrollbar, so the right scrollbar, mouse wheel and page scrolling stay in sync.

## Tabs

- The redundant **Classic configuration** tab was removed. The **Classic configuration** button in the overview remains.

## Validation

- Fixture checks: **276/276** on x64.
- Verified: single selection, persistent highlight after mouse leave, fast tile switching, page scrolling through the complete plugin list, Classic tab removed.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
