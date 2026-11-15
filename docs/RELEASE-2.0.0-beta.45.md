# MP1 Configuration Studio 2.0.0-beta.45

**Status:** Forty-fifth preview. Rebuilds the 1.9 toolbar in the new interface, adds English as the default language, a light theme, and a clickable pending-changes list. Unsigned preview.

## 1.9-style toolbar

- Back and forward history buttons.
- Search field with magnifier icon and `Ctrl+F` hint; `Ctrl+F` focuses the field.
- **Standard / Expert** view mode. Standard hides raw XML, plugin data, special areas/files, keyboard, views and DSP; Expert shows every page. Switching to Standard from an advanced page navigates to the first visible page.
- Text-size selector, **Dark / Light** theme selector, help button (keyboard shortcuts) and a Tools menu.
- **Tools** menu: back up now, manage backups, English/Deutsch, classic interface, 1.9 interface, keyboard shortcuts, support bundle, about.
- The toolbar caps its fixed-width controls at high text scales so the search field remains usable at 175 %.

## Language

- **English is now the default.** Existing preferences migrate to English once (appearance schema 3); German remains selectable under Tools → Deutsch. Switching the language restarts the interface in place and keeps the current page.
- French and Dutch are not included yet; they need a translation pass over the schema labels.

## Light theme

- The Dark/Light selector uses the existing light palette (light grey surfaces, white panels, dark text) across the native pages and structured editors. The window title bar follows the theme.
- Theme changes persist and restart the interface at the current page.

## Feedback and pending changes

- **Backup** and **Save** now report their result in accent colour in the footer for six seconds (for example `Backup created · native-manual-…zip`) and then restore the Ready/Unsaved status.
- **Unsaved changes** is clickable. It opens a dialog listing every pending `section / entry` with old → new value, including entries added or removed, plus the number of XML editors with unsaved text.
- Change tracking is kept in `StudioConfiguration` and cleared on save; reverting a value removes it from the list.

## Sidebar scrolling

- The sidebar scroll host reserved one horizontal scrollbar height too many, so the list could not reach its last entry and the 1.9 interface entry was clipped. The host now sizes the content exactly; the scrollbar reaches the end and the last entry is fully visible.
- Sidebar height calculation uses the real child positions, so scrolling up works after reaching the bottom.

## Validation

- Fixture checks: **258/258** on x64 (pending-change tracking, light palette, pending dialog).
- Verified: toolbar at 100 % and 175 %, English default, German option, dark/light switch with restart, backup status message, pending-changes dialog, sidebar scroll to bottom and back, back/forward history.
- Release workflow remains green.

## Install

The combined setup installs application and optional plugin together. No configuration format changes.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
