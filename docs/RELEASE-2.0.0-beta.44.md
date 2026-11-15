# MP1 Configuration Studio 2.0.0-beta.44

**Status:** Forty-fourth preview. Fixes the stale sidebar highlight (the reported hover problem) and finishes the dark scroll and slider controls. Unsigned preview.

## Hover / selection

- Clicking a page changed the `Selected` flag on all sidebar items, but only the container was invalidated. The previously selected item kept its accent bar and fill until it happened to repaint, which looked like a stuck hover or a second selection.
- The Studio now invalidates each navigation item whose selection actually changes. Only the current page shows the accent bar and selection fill; hover stays a separate, temporary state.

## Remaining light controls removed

- **Scroll bars:** keyboard, raw XML, views detail and DSP editors use the slim dark scroll host instead of the native light bars.
- **DSP sliders:** the native white track bars for Gain and Compressor are replaced by a dark slider with accent thumb, tick marks, mouse drag and keyboard support (arrow keys, Home/End, Shift for larger steps).

## Validation

- Fixture checks: **254/254** on x64.
- Hover/selection sequence verified: select Video, select Lautstärke, hover Startmenü – only Lautstärke stays selected.
- Raw XML entries, view detail and DSP cards scroll with the dark thumb; DSP values still load and save.
- Scale screenshots remain valid from beta.43 (100–175 %).

## Install

No configuration format changes. The combined setup installs application and optional plugin together. This build was not installed automatically because no further UAC prompt was desired.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
