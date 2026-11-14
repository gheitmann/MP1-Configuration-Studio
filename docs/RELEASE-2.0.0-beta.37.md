# MP1 Configuration Studio 2.0.0-beta.37

**Status:** Thirty-seventh preview. Reworked the visual layer of the new interface: real glass surfaces, custom selection controls, larger interactive check boxes, and the subsection tabs from 1.9. Unsigned preview.

## Glass now shows the desktop

The old glass mode painted a light gray gradient over the chrome, which read as a frame rather than glass. The chrome (header, sidebar, footer) now samples a blurred, dark-tinted copy of the desktop wallpaper and follows the window position, so the surfaces actually look translucent. Toggling **Glas** switches between the glass material and the flat dark shell; High Contrast always uses a solid surface. If Windows has no wallpaper configured, a dark glass gradient is used instead.

## Selection controls

- Native white-framed combo boxes are replaced by an owner-drawn dropdown: rounded border, accent focus ring, chevron, dark popup list, accent bar and check mark for the current value.
- The popup supports mouse, wheel, Up/Down, Enter and Esc, and sizes itself to the longest entry (capped to the screen).
- Text inputs use the same rounded frame instead of the white classic border.
- Check boxes are larger, draw a real check mark, and show hover, pressed, checked and keyboard-focus states.
- Content and sidebar scroll bars are slim dark thumbs instead of light native scroll bars.

## Hover

Navigation items, subsection tabs, dropdowns, check boxes, text frames and footer buttons all paint hover feedback. Check boxes also tint their whole row while hovered or focused, so it is obvious which setting the pointer is over.

## Subtabs from 1.9

Long pages are split into the tab row known from the 1.9 interface. Every group of a page becomes a tab (plus **Weitere Einstellungen** when unknown entries exist); the selected tab is remembered per page. With many groups the tabs wrap into several rows at 100–175 % text scale. A search that matches a field selects the tab containing it.

## Fixes

- A whitespace-only value in `MediaPortal.xml` no longer creates a blank, unselectable entry in a dropdown; the first real option is shown instead.
- The 1.9 interface keeps working unchanged; the shared scroll host gained an optional background colour.

## Validation

- Fixture checks: **253/253** on x64.
- New checks cover the dropdown value mapping (including whitespace), the large check box layout, the 1.9 tab strip, the glass material, and subsection switching on the Video page.
- Screenshots verified at 100 % and 175 % text scale: glass on/off, wrapped tab rows on **Spezialbereiche**, dropdown popup, mouse hover on navigation/tabs/check boxes, scrolled content and sidebar.
- Existing page matrix still builds every schema section at 100/125/150/175 %.

## Install

The combined setup installs the application and the optional MediaPortal plugin with the same version. The standalone plugin ZIP remains available for manual install/uninstall.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
