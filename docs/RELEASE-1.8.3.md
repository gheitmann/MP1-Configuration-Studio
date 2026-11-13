# MP1 Configuration Studio 1.8.3

**Status:** Tools-menu stability and layout-space release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## Fixes in 1.8.3

- **Tools menu crash fixed:** the `ContextMenuStrip` is no longer disposed while a submenu can still reference it. Opening Tools and its submenus no longer throws `ObjectDisposedException`.
- **Tab variant 9:** larger fine-card tabs with a thin outline; the active tab uses the accent border and accent text. The previous folder tabs were too small.
- **More content width:** the right help column is hidden by default. Use the top-right help icon or Tools → Show help column to bring it back. Hiding it removes horizontal pressure and several native scrollbars.
- **Disabled labels readable:** label-like native controls are rendered as normal light text even when MediaPortal marks them disabled; the controls themselves remain non-interactive.
- Includes all 1.8.2, 1.8.1, 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 fixes.

## Validation

- Fixture checks: 207/207 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Page audit: screenshots, low-contrast and overlap diagnostics.
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
| High Contrast | System palette and system-color owner drawing; a dedicated visual session remains pending |
| Plugins | Native pages Tier A; unknown plugins Tier B; Tier C blocked |

## Native layout note

Fragile native pages keep their original MediaPortal geometry and font. Hiding the help column gives those pages more width. Individual controls can still be clipped on pages with absolute MediaPortal coordinates; the classic interface remains the fallback for a fully original layout.

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
- Some native pages use fixed absolute coordinates and can still clip content.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, and clean-VM checks remain pending.
- NUnit migration is deferred.
- Unsigned until SignPath approval.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
