# MP1 Configuration Studio 2.0.0-beta.53

**Status:** Fifty-third preview. Fixes footer button spacing and makes plugin overview failures visible and recoverable instead of showing an empty list. Unsigned preview.

## Footer

- Added spacing between **Backup** and **Save** so the primary button no longer touches the neighbouring button.

## Plugin overview

- The plugin scan now waits up to three minutes instead of one before giving up.
- If the scan fails or times out, the page shows the reason and the plugin path instead of a bare “No plugins found”.
- The plugin overview logs the runtime path, exit code and number of detected plugins to the Studio log.
- **Refresh** retries the scan and forces a fresh list after installing or removing plugins.
- The 12-hour cache avoids a long scan on every visit once the first scan has completed.

## Validation

- Fixture checks: **276/276** on x64.
- Verified with the installed application and production MediaPortal runtime: 44 plugins detected, icon grid and action bar visible.
- Footer spacing checked visually.

## Install

The combined setup installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
