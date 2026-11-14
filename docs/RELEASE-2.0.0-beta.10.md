# MP1 Configuration Studio 2.0.0-beta.10

**Status:** Tenth preview. The native interface is the default entry point; the 1.9 Studio shell is now reachable from the native sidebar. Unsigned preview.

## New in beta.10

- The native sidebar now offers **Studio 1.9-Oberfläche** as a built-in fallback next to **Klassisch / Plugins**.
- Choosing it closes the native window and starts the 1.9 Studio shell with the same runtime and preferences; the single-instance lock is released first.
- The unsaved-changes prompt is shown once and no longer duplicated when leaving for the classic or 1.9 interface.

## Starting modes

| Start | Interface |
| --- | --- |
| no arguments | Native Studio interface (default) |
| `--native` | Native Studio interface |
| `--section=<id>` | Native Studio interface on a specific page |
| `--studio` | 1.9 Studio shell |
| `--classic` | Original MediaPortal `Configuration.exe` |

## Validation

- Fixture checks: **220/220** on x64.
- No-argument start verified: **MP1 Configuration Studio · Neue Oberfläche**.
- `--studio` verified: **MP1 Configuration Studio · MediaPortal 1.39**.
- Sidebar **Studio 1.9-Oberfläche** verified: the native window closed and the 1.9 shell attached successfully (`Studio shell attached`, 299 ms).
- Production `MediaPortal.xml` was not modified.

## Still classic

- Plugin configuration windows.
- Sorting/layout internals for music and video views.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe` — the native interface opens by default.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
