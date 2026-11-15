# MP1 Configuration Studio 2.0.0-beta.38

**Status:** Thirty-eighth preview. Removed the experimental glass mode and returned to the flat 1.9 blue/black chrome; polished the subsection tabs. Unsigned preview.

## Changes

- **Glass removed.** The blurred-wallpaper mode is gone, including the header toggle. Header, sidebar and footer use the flat palette from the 1.9 interface again: dark navy background, slightly lighter panel surfaces and the existing blue accent. The persisted `glass` preference is ignored.
- **Subsection tabs:** the blue underline below the active tab is removed; the active tab is still highlighted by a filled pill and accent text.
- **Subsection tabs:** more distance between the tab rows and the first setting, so labels no longer sit directly under the tabs.
- Search field and text-size selector share the header directly again; the search field gained the space the glass toggle used.
- Scroll bars, custom dropdowns, large check boxes and hover states from beta.37 are unchanged.

## Validation

- Fixture checks: **252/252** on x64 (the glass-material check was removed with the feature).
- Screenshots verified at 100 % and 175 % text scale: flat chrome, tab rows without underline, spacing above the first setting, dropdown and check-box rendering, sidebar/content scrolling.
- Every schema section still builds at 100/125/150/175 %.

## Install

The combined setup installs the application and the optional MediaPortal plugin with the same version. The standalone plugin ZIP remains available for manual install/uninstall.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
