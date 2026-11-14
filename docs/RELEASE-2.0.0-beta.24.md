# MP1 Configuration Studio 2.0.0-beta.24

**Status:** Twenty-fourth preview. Trakt and OnlineVideos now have structured forms. Unsigned preview. Not installed locally (no UAC run).

## New in beta.24

### Trakt form

The Plugin-Daten page now includes a grouped Trakt editor with all 211 entries:

- **Konto:** version, username, access/refresh tokens
- **Synchronisierung:** sync interval, start delay, library clean, blocked names/folders
- **Scrobbling / Plugins:** Moving Pictures, TV-Series, MyVideos, MyFilms, OnlineVideos, TV recordings/live, Argus
- **Layouts & Kalender:** all default layouts and calendar defaults
- **Sortierung:** all `SortBy*` JSON values
- **Empfehlungen:** movie/show recommendation settings
- **Dashboard:** polling and delay settings
- **Netzwerk & Fanart:** fanart downloads, cache/timeouts
- **Verhalten & Datenschutz:** hide/rating/friend-request options
- **Weitere:** everything not classified above

Every entry is editable exactly once; `yes`/`no` becomes a checkbox. Search filters by key or value.

### OnlineVideos form

- Views: `groupview`, `siteview`, `siteview_order`, `videoview`, `categoryview`
- History: `searchHistory`, `lastFirstRun`
- Editable directly; search filters entries.

## Validation

- Fixture checks: **224/224** on x64.
- OnlineVideos round-trip: changed `groupview` to `3`, saved, verified in `MediaPortal.xml`.
- Trakt round-trip: changed `MovingPictures` to `5`, saved, verified in `MediaPortal.xml`.
- Both forms write through `StudioConfiguration`; unknown entries remain preserved.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
