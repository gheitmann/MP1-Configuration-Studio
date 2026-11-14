# MP1 Configuration Studio 2.0.0-beta.28

**Status:** Twenty-eighth preview. Adds a read-only EPG overview for TV and radio channels to the TV Server details. Unsigned preview. Not installed locally (no UAC run).

## New in beta.28

### EPG overview (read-only)

The `--tv-overview` helper now reads programmes for the next 12 hours through the official TV Server business layer (`TvBusinessLayer.GetPrograms(start, end)`) and emits one line per channel:

```
epg=tv|Studio One HD|20:15|21:05|Tagesschau|21:05|22:00|Tatort
```

- Current programme: start, end, title
- Next programme: start, end, title
- Empty fields when the guide has no data

The **TV-Server-Details** dialog now has two additional categories:

- **TV-EPG 12 h**
- **Radio-EPG 12 h**

Each entry is rendered as:

```
Studio One HD — jetzt: Tagesschau (20:15–21:05)  ·  danach: Tatort (21:05–22:00)
```

If the guide has no data for a channel, it shows `— keine EPG-Daten` instead of an empty line.

The helper also reports the number of programmes found in a ±7-day window as `epgPrograms=` for diagnostics.

### Safety

- No writes to the TV database.
- No new SetupTv replacement.
- If the program query fails or the guide is empty, the page and dialog continue to work.

## Validation

- Fixture checks: **227/227** on x64.
- New EPG checks:
  - helper line format with current and next programme
  - dialog rendering with current and next title/times
  - graceful `keine EPG-Daten` rendering
- Helper output on the reference system: 16 TV + 4 radio channel EPG lines, `epgPrograms=0` (the test server has no guide data), exit code 0.
- Dialog screenshot: **TV-EPG 12 h (16)** with all channels marked `keine EPG-Daten`.
- Production configuration untouched.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
