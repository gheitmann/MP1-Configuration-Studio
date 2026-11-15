# MP1 Configuration Studio 2.0.0-beta.51

**Status:** Fifty-first preview. Restores the classic menu order and topics, makes the project page the start page, adds donation guidance, and uses official MediaPortal artwork. Unsigned preview.

## Classic menu order

The native sidebar now follows the classic MediaPortal configuration again:

- **Project** (start page)
- **General** — General, Volume, Keyboard shortcuts
- **GUI** — GUI, Start menu
- **Videos** — Videos, Video database, Video views, Subtitles
- **Blu-ray / DVD**
- **TV / Radio** — TV, Radio, TV recordings
- **Music** — Music, Music files, Music views, Music DSP
- **Pictures**
- **Media folders**
- **Remotes & Input**
- **Codecs & Renderer**
- **Plugins** — Plugins, Plugin data
- **Media info**
- **3rd-party checks**
- **More areas** — Special areas, Special files, Raw XML

Group captions are shown only where the classic tree has several related pages. Long titles use short sidebar labels so they no longer end in an ellipsis. In Standard mode, advanced groups and captions hide together.

## Project start page

- The application now opens on **Project**, matching the classic configurator.
- The page shows the MediaPortal description, official links (homepage, forum, documentation, GitHub, IRC), the Studio project link and version information.
- The donation box explains that donations help the MediaPortal project with hosting, development and infrastructure. The **Donate via PayPal** button and the visible link both open `https://forum.team-mediaportal.com/donate/`.

## Topics aligned with classic

- CD/DVD speed and Daemon Tools moved back to **General**.
- **Media info** is a separate topic instead of being buried under Special areas.
- **System checks** is now the **3rd-party checks** page; it contains only the classic third-party checks.
- Renamed topics: **GUI**, **Videos**, **Remotes & Input**, **Codecs & Renderer**, **Blu-ray / DVD**.

## Artwork

- The Studio header and About window use the official MediaPortal icon from the MediaPortal website.
- The Project page uses the official MediaPortal logo artwork.
- The classic press packshots are licensed for personal use only, so they are not redistributed with the application.

## Validation

- Fixture checks: **275/275** on x64, including project start page, classic order, media info topic and stable subsection switching.
- Verified visually at 100 % and 175 % text scale: Project start page, grouped sidebar, General with CD/DVD and Daemon Tools, 3rd-party checks.
- Production `MediaPortal.xml` was not modified.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal logo artwork is used for project identification only; MediaPortal is not bundled with this application.
