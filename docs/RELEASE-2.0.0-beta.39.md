# MP1 Configuration Studio 2.0.0-beta.39

**Status:** Thirty-ninth preview. Gives the subsection tabs real breathing room above the settings and fixes wrapped tab rows. Unsigned preview.

## Changes

- **Tab spacing:** an explicit spacer row now separates the tab strip from the first setting (26 px at 100 %, scaled with the text size). The previous margin was swallowed by the table layout, so fields could sit directly under the tabs.
- **Wrapped tabs:** the tab strip is no longer stretched by its table row, so two or three rows of tabs always reserve their full height. Previously a wrapped row could overlap the content below.
- Tabs keep the flat 1.9 look: active tab as a filled pill with accent text, no underline. Row gap between wrapped tabs increased slightly.

## Validation

- Fixture checks: **252/252** on x64.
- Screenshots at 100 %: single-row page (`System und Laufwerke`), two-row page (`Allgemein`), three-row page (`Spezialbereiche`); content always starts below the last tab row plus the spacer.
- 175 % scaling still wraps and spaces correctly.

## Install

The combined setup installs the application and the optional MediaPortal plugin with the same version. The standalone plugin ZIP remains available for manual install/uninstall.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
