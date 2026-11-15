# MP1 Configuration Studio 2.0.0-beta.50

**Status:** Fiftieth preview. Completes the classic settings audit outside Volume, adds the missing General, Video, Music, subtitle, disc and remote fields, and makes subsection switches stay on the same page instead of rebuilding it. Unsigned preview.

## Classic settings audit

Compared all classic 1.39 configuration sections with the Studio pages and imported the remaining static settings:

- **General:** restart delay, madVR frame reduction, TV refresh-rate profile, custom refresh-rate entries, monitor device ID.
- **System:** CD/DVD speed master switch, complete Daemon Tools configuration (program, extensions, drive letter, drive number, drive type).
- **Interface:** thumbnail quality, on-demand thumbnail options, thumbnail columns/rows and pre-record interval.
- **Video:** playlist folder, fuzzy title matching, external-player extension filter, LAV splitter and FFDShow audio-delay step.
- **Video database:** remove title prefixes from sort titles, movie info before playback and in share view, plus grabber entries.
- **Music:** sound device ID, ASIO balance, resume position/search, and the classic “jump after play” modes.
- **Subtitles:** the four auto-load modes and the five classic subtitle engines.
- **BD/DVD:** parental control level, internal Blu-ray menu and MediaInfo usage. AAC codec added to the Blu-ray audio group.
- **Remote:** FireDTV device name.
- **Special areas:** complete file-menu, USBUIRT, SerialUIR, WinLIRC, RedEye and MediaInfo cache settings. Plugin extensions moved to Plugin data.

Fixed two correctness issues found during the audit: autoplay now uses the classic lowercase values (`ask what to do`, `play`, `do not play`) instead of invented `Ask`/`Yes`/`No` values, and the duplicate aspect-ratio field that wrote to the wrong `movies` key was removed.

Only internal markers, legacy migration keys and dynamic IR-learning entries remain outside the curated schema; those are either not user settings or use dynamic editors.

## Smoother subsection switching

- Switching **Control / Startup / On-screen display** or any other subsection now replaces only the content area. The page title, subsection strip and the selected sidebar entry stay in place; the sidebar underline is no longer reloaded.
- Scroll position is preserved when changing subsections.
- The content table and scroll host use double buffering to avoid intermediate white frames.
- Subtab content is disposed when replaced, so repeated switching does not leak controls.

## Plugins

- Plugin data now also exposes installed GUI extensions (`myextensions2`) alongside OnlineVideos, Trakt, IMDb+, Last.fm, FFDShow, mpSettings and menu data.
- The plugin configuration panel continues to detect every installed plugin with its own setup window.

## Validation

- Fixture checks: **272/272** on x64, including classic option values and stable subtab instances.
- Verified visually: General, System, Video and Special areas pages; subsection switching keeps the Volume sidebar selection and header stable.
- Production `MediaPortal.xml` was not modified during testing.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
