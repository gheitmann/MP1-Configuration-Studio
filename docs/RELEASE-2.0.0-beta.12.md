# MP1 Configuration Studio 2.0.0-beta.12

**Status:** Twelfth preview. The native interface is the default; the 1.9 shell is available from the sidebar. Unsigned preview.

## New in beta.12

### Video-Datenbank

- New page for title and search behavior, fanart, actors, and scan options.
- Includes sort title, folder-as-title, filename search preference, title prefixes, fanart count, skip-existing, NFO-only scraping, and actor fetching.

### Musik-Dateien

- New page for music playback, playlists, and database import.
- Playback: repeat, auto shuffle, songs to load, selection action.
- Playlists: save on exit, resume on open, current playlist, UTF-8 saving.
- Import and thumbnails: artist, genre, and folder thumbnails, folder-as-album, share monitoring, incremental import, artist prefixes.

### Video player

- New **Player** group on the Video page: OSD timeout, zap delay/timeout/key timeout, skip steps and relative skip size, preferred audio language, forced audio language, LAV/stream and WMV audio, default aspect ratio, internal/external player path and arguments, automatic commercial skipping, and audio-delay reset.

## Validation

- Fixture checks: **220/220** on x64.
- All new pages inspected at 125% with values from the live configuration.
- Video player group inspected at 125%: `Normal` aspect ratio, internal player enabled, comskip/audio-delay options shown.
- Production `MediaPortal.xml` was not modified.

## Still classic

- Plugin configuration windows.
- Music view column templates and other advanced list customisation.
- The 1.9 shell remains available from the sidebar or with `--studio`.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe` — the native interface opens by default.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
