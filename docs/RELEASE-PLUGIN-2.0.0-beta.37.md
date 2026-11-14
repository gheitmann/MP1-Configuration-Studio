# MP1 Configuration Studio plugin 2.0.0-beta.37

**Status:** Preview aligned with desktop Studio beta.37. Unsigned preview.

## New

- The MediaPortal plugin is unchanged in behaviour and keeps its own skin, but ships with the same version as the desktop Studio.
- Shared schema and choice catalog are unchanged, so desktop and plugin still offer identical options.

## Verified

- Plugin build: 0 errors.
- Plugin smoke test: window `8917`, home page and description render.
- 253/253 fixture checks for the combined source tree.

## Limits

- Hardware/driver/plugin-dependent lists (BASS sound devices, FireDTV names, TV hostnames, idle-plugin window IDs) remain editable.
- Unsigned; changes require a MediaPortal restart.
