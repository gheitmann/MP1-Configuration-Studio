# MP1 Configuration Studio plugin 2.0.0-beta.40

**Status:** Build fix aligned with desktop Studio beta.40. Unsigned preview.

## Changes

- No plugin behaviour changes; version aligned with the desktop Studio.
- Pinned C# language version to keep GitHub Actions and local builds identical.

## Verified

- Plugin build: 0 errors.
- Plugin smoke test: window `8917`, home page and description render.
- 252/252 fixture checks for the combined source tree.

## Limits

- Hardware/driver/plugin-dependent lists (BASS sound devices, FireDTV names, TV hostnames, idle-plugin window IDs) remain editable.
- Unsigned; changes require a MediaPortal restart.
