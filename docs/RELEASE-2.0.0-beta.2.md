# MP1 Configuration Studio 2.0.0-beta.2

**Status:** Second preview of the new native Studio interface. The stable 1.9.0 interface remains the default. Unsigned preview.

## Fixes in beta.2

- The native UI now finds the production `MediaPortal.xml` under `C:\ProgramData\Team MediaPortal\MediaPortal` as well as the isolated runtime layout. The first beta could only open test runtimes.
- Sidebar label shortened to “Klassisch / Plugins”.
- Includes the beta.1 foundation: native settings shell, safe configuration service with backup/atomic save, and six core pages.

## Implemented core pages

- General
- Volume
- Pictures
- Interface
- Video
- Music

## Scope

This is still a foundation preview:

- Media folder/share lists remain classic-only.
- TV/Radio, codecs, plugins, and the remaining native sections are not rebuilt yet.
- Plugin windows keep their own layout.
- The stable 1.9.0 shell remains the default; the new UI is under Tools → New interface (beta), or start with `--native`.

## Validation

- Fixture checks: 213/213 on x64.
- Stable native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Configuration round-trip preserves unknown sections and creates a backup.
- New UI constructs with all six core sections.

## Known limitations

- Not all core settings or plugin configuration are covered.
- Media folder/share lists remain classic-only.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, clean-VM checks, and SignPath approval remain pending.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
