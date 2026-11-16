# MP1 Configuration Studio 2.0.0-beta.63

**Status:** Sixty-third preview. New "Start MediaPortal" action in the Tools menu. Unsigned preview.

## Tools menu

- New entry **"MediaPortal starten" / "Start MediaPortal"**: starts `MediaPortal.exe` from the MediaPortal folder and closes the Studio. Unsaved changes still ask first (save or discard), so nothing is lost by accident.
- If `MediaPortal.exe` is missing or cannot be started, the Studio stays open and shows a clear message instead.
- Localized in German, English, French ("Lancer MediaPortal"), and Dutch ("MediaPortal starten"); regression tests pin the strings in all four languages (285 checks).

## Validation

- Fixture checks: **285/285** on x64.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. MediaPortal artwork is used for identification only.
