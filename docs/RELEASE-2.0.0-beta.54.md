# MP1 Configuration Studio 2.0.0-beta.54

**Status:** Fifty-fourth preview. Fixes the plugin overview for the installed MediaPortal path and keeps the footer spacing fix. Unsigned preview.

## Plugin overview fix

The installed application resolves its MediaPortal runtime with a trailing directory separator. That separator escaped the closing quote when the plugin scanner was started, so the child process received a malformed path and exited with no plugin list.

- The runtime path is now normalized without a trailing separator.
- All plugin helper arguments use command-line-safe quoting that handles trailing backslashes.
- The plugin configuration and DLL-list helpers use the same quoting.
- The overview now reports the real scanner error if a future failure occurs.
- The installed default path therefore resolves to:
  `C:\Program Files\Team MediaPortal\MediaPortal`

## Footer

- The spacing between **Backup** and **Save** remains corrected.

## Validation

- Fixture checks: **276/276** on x64.
- Verified the installed runtime path form and the quoted plugin-scanner argument with a trailing separator.
- Production `MediaPortal.xml` was not modified by the Studio.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
