# MP1 Configuration Studio 1.4.0

**Status:** Safety and restart release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## New in 1.4

- **Unsaved-changes guard:** closing the Studio window with pending edits now asks for confirmation and defaults to keeping Studio open. Programmatic closes, the explicit Cancel button, and Studio-initiated restarts are not affected.
- **Automatic restart after a DPI change:** when Windows reports a display scaling change, Studio offers to restart immediately so the shell and native pages are laid out at the new scale. The restart runs after the DPI event has completed.
- **Manual restart:** Tools → Restart Studio closes and relaunches Studio. It warns when unsaved changes would be lost.
- Includes all 1.3, 1.2, 1.1, and 1.0.1 features.

## Validation

- Fixture checks: 197/197 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Attach time: about 340 ms.

## Supported matrix

| Item | Support |
| --- | --- |
| MediaPortal | 1.39 x64 |
| Windows | Windows 11 23H2 or newer |
| Runtime | .NET Framework 4.8 |
| Languages | German complete; English complete except MediaPortal/plugin-owned dialogs |
| Internal scaling | 100, 115, 125, 150, 175% |
| OS DPI | 100% validated; 150/200% still need a fresh-session check |
| Plugins | Native pages Tier A; unknown plugins Tier B; Tier C blocked |

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the setup or portable ZIP and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe`.

## Code signing policy

Free code signing provided by SignPath.io, certificate by SignPath Foundation. The application will be submitted after the planned repository move toward the official MediaPortal organization.

- Committers and reviewers: [@gheitmann](https://github.com/gheitmann)
- Approvers: [@gheitmann](https://github.com/gheitmann)
- Privacy policy: [PRIVACY.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/PRIVACY.md)
- SignPath setup: [docs/SIGNPATH.md](https://github.com/gheitmann/MP1-Configuration-Studio/blob/main/docs/SIGNPATH.md)

## Known limitations

- Native MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI support is still pending. Studio detects a reported DPI change and offers a restart; OS DPI 150/200% fresh-session checks remain pending.
- The NUnit migration is deferred. The existing 197-check fixture harness and the native smoke harness remain the release gate; a thin NUnit wrapper would not add separate test cases.
- Clean Windows 11 VM install checks remain pending.
- Unsigned until SignPath approval.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
