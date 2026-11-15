# MP1 Configuration Studio 2.0.0-beta.56

**Status:** Fifty-sixth preview. Grows the plugin action bar instead of clipping the last button, synchronizes the page scrollbar after wheel scrolling, and improves Project readability. Unsigned preview.

## Plugin overview

- The action row now grows automatically when its buttons wrap, so **Classic configuration** and every other action stay fully visible.
- The overview uses the page scrollbar only; the right scrollbar is synchronized after mouse-wheel scrolling and reaches the bottom of the list.
- The overview reserves enough height for all window, external and process plugin groups at 100–175 % scaling.
- The fixed footer spacing and single-selection behavior from beta.55 are unchanged.

## Project page

- Body, link, heading and donation text are larger and easier to read at 100 %.
- **Contact** was renamed to **Project links** / **Projekt-Links**, since the section contains links rather than a contact form.
- The topbar was left unchanged as requested.

## Validation

- Fixture checks: **276/276** on x64.
- Verified: action row without clipping, page scrolled to the last process plugin with the scrollbar thumb at the bottom, larger Project text and renamed links section.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
