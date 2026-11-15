# MP1 Configuration Studio 2.0.0-beta.43

**Status:** Forty-third preview. Finishes the visual unification of the structured editors and turns the Windows title bar dark. Unsigned preview.

## Editors now use the Studio controls

- **Keyboard:** context selector, action filter and per-action sound field use the shared dropdown/text frames; the add-action dialog got the same editable sound combo.
- **Views:** view search, filter fields (field, operator, value, sort, view, limit), ascending/distinct/skip-level check boxes and the filter dialog use the Studio controls. The view list draws a dark selection with an accent bar instead of the bright system highlight and has no white border.
- **Raw XML:** section selector, section filter, entry fields and boolean entries use the shared controls; entries render again after the selector migration.
- **Plugin data, TV details, DSP, XML files:** plugin selector, TV category dialog, music folder, DynAmp preset, effect/VST names, parameter fields and XML file selector are converted.
- **Buttons:** every structured-editor and dialog button is now the rounded `StudioButton` with hover and focus states.

The standalone 1.9 shell keeps its own controls by design.

## Window

- The title bar is dark on Windows 10/11. The manifest now declares the supported OS versions, and the dark-mode attribute is applied to both DWM attribute 20 and the older 19 on creation and activation.

## Accessibility

- New dropdowns, editable combos, text frames and check boxes expose accessible roles; check boxes derive their accessible name from their label.
- High Contrast keeps the selected subsection tab readable by using the system highlight text colour.

## Validation

- Fixture checks: **254/254** on x64 (new checks for the editable combo and Studio button).
- Screenshots verified at 100 %, 115 %, 125 %, 150 % and 175 % text scale; keyboard, views, raw XML and DSP pages reviewed.
- Dark title bar confirmed on the running window.

## Install

No configuration format changes. The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
