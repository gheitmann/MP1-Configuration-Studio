# MP1 Configuration Studio 2.0.0-beta.18

**Status:** Eighteenth preview. Adds the read-only TV Server overview on top of the beta.17 keyboard, views, and DSP editors. Unsigned preview. Not installed locally (no UAC run).

## New in beta.18

### Read-only TV Server overview

The **TV** page now shows live infrastructure data without writing anything:

- TV Server installation and `TVService` state.
- Database provider, database name, and server from `Gentle.config` (passwords are never displayed).
- Read-only counts from the official TV Server business layer:
  - TV channels
  - Radio channels
  - TV groups
  - Radio groups
  - Cards/tuners
  - Recordings
  - Schedules/timers
- Example on the reference system: **TV: 20 Kanäle · Radio: 4 · 3 Gruppen · 72 Aufnahmen · 0 Timer · 2 Karten**.

The counts are produced by an isolated Studio helper process (`--tv-overview`) that loads `TvBusinessLayer.dll` and `TVDatabase.dll` from the installed TV Server, runs with the TV Server configuration directory, and returns only numeric results. It does **not** write to the TV database and does **not** control the TV service.

### SetupTv scope

A complete replacement of `SetupTv` is still not included. Channel scanning, CAM/CI, EPG management, card mapping, schedules, and TV Server plugins remain in the TV Server administration application. Direct database writes are intentionally avoided; if write support is added later, it should use the TV Server business layer with explicit service coordination.

## Carried over from beta.17

- Native **Tastatur** editor for `keymap.xml` with key capture and duplicate warnings.
- Native **Musik-Ansichten** and **Video-Ansichten** editors with localized names, filter cards, reordering, and SOAP-safe saves validated through MediaPortal's `SoapFormatter`.
- Native **Musik-DSP** editor for music folder, BASS effects, and VST/Winamp plugins.
- XML file editor, raw XML section editor, DirectShow filter property pages, and classic plugin configuration.

## Validation

- Fixture checks: **222/222** on x64.
- TV overview helper verified against the installed TV Server: 20 TV channels, 4 radio channels, 3 TV groups, 2 radio groups, 72 recordings, 0 schedules, 2 cards.
- TV page inspected with the counts and database summary; `MediaPortal.xml` hash unchanged.
- Keymap, views (including `SoapFormatter` round-trip), DSP, XML file editor, and filter property pages verified in beta.16/beta.17.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
