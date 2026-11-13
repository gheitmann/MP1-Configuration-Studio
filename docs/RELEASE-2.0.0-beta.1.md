# MP1 Configuration Studio 2.0.0-beta.1

**Status:** Preview of the new native Studio interface. The stable 1.9.0 interface remains the default; the new UI is available under Tools → New interface (beta). Unsigned preview.

## What is new

- **New native settings shell** that no longer re-hosts or re-themes the original MediaPortal pages.
- Dark Studio design based on the 1.8 appearance: Segoe UI, clear hierarchy, responsive field rows, no fixed native coordinates.
- **Safe configuration service:**
  - reads and writes `MediaPortal.xml`
  - preserves unknown sections and entries
  - creates a ZIP backup before every save
  - atomic write through a temporary file
- **Core pages implemented in the beta:**
  - General
  - Volume
  - Pictures
  - Interface
  - Video
  - Music
- **Classic/plugins fallback:** the original `Configuration.exe` remains available from the new UI and from the stable shell.
- Theme size selector 100–175% in the new UI; fields reflow instead of overlapping.

## Scope and limitations

This is a **foundation preview**, not a full replacement:

- Media folders (shares) are still edited in the classic interface.
- TV/Radio, codecs, plugins, and the remaining native sections are not rebuilt yet.
- Plugin windows keep their own layout; that is intentional.
- The stable 1.9.0 shell remains the default in this beta.
- Save semantics for plugin configuration files are not reimplemented.

## Validation

- Fixture checks: 213/213 on x64.
- Native smoke (stable shell): 27/27 on isolated MediaPortal 1.39 x64.
- Native configuration round-trip preserves unknown sections and creates a backup.
- New UI constructs successfully with the six core sections.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Run the setup or extract the portable ZIP.
4. Start `MP1ConfigurationStudio.exe` for the stable shell, or Tools → New interface (beta) for the native preview.

## Known limitations

- The beta does not yet cover all core settings or plugin configuration.
- Media folder/share lists remain classic-only.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, clean-VM checks, and SignPath approval remain pending.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
