# MP1 Configuration Studio 2.0.0-beta.62

**Status:** Sixty-second preview. Complete French and Dutch localization — the whole interface, the full settings schema, and every dialog. Unsigned preview.

## Languages

- The Studio now ships completely in four languages: **German, English, French, and Dutch**.
- Translated: every settings field label (543), all field help texts, section titles and subtitles, group and subsection titles, combo option labels, the sidebar, the plugin overview (tiles, actions, status line), all dialogs (pending changes, keyboard shortcuts, first run, backup manager, error and about), and the MediaPortal plugin pages.
- Field tooltips follow the language as well: help text or the "Default: …" fallback is localized.
- The language is chosen in the Tools menu ("Français", "Nederlands") or the classic Studio menu; the choice is stored in the appearance preferences. Missing strings fall back to English, never to untranslated German.
- Regression tests in the fixture suite now pin the French and Dutch lookups (283 checks).

## Plugin

- The MediaPortal plugin follows the MediaPortal UI culture: French and Dutch render the same translated schema, labels, options, and status messages.

## Validation

- Fixture checks: **283/283** on x64 (including French and Dutch string and schema lookups).
- Verified at runtime: the General and Plugins pages render fully in French and in Dutch (topbar, subsection tabs, labels, help texts, footer actions).
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal artwork is used for identification only.
