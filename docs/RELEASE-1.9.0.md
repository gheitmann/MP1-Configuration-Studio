# MP1 Configuration Studio 1.9.0

**Status:** Usability release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## New in 1.9

- **Native pages use the original MediaPortal look by default.** The shell stays dark, but native pages keep their original control colors, geometry, and typography. This removes the black-on-dark text and most overlap/empty-area failures caused by re-theming and scaling fixed native layouts.
- The mode can be switched under Tools → Native pages in original look. Switching requires a restart.
- **Tab variant 9:** larger fine-card tabs with a thin outline and accent state.
- **Help column hidden by default**, giving pages more width and reducing scrollbars. Restore it with the top-right help icon or Tools → Show help column.
- **Tools menu stability:** the `ContextMenuStrip` lifetime bug is fixed; submenus no longer throw `ObjectDisposedException`.
- **Save flow:** the native OK/Save path no longer triggers the unsaved-changes discard prompt.
- Includes all 1.8.4, 1.8.3, 1.8.2, 1.8.1, 1.8.0, 1.7, 1.6, 1.5, 1.4, 1.3, 1.2, 1.1, and 1.0.1 fixes.

## Validation

- Fixture checks: 207/207 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Page audit with original native look: 61 screenshots, 0 low-contrast controls, 2 native overlap candidates.
- Attach time: about 340 ms.

## Supported matrix

| Item | Support |
| --- | --- |
| MediaPortal | 1.39 x64 |
| Windows | Windows 11 23H2 or newer |
| Runtime | .NET Framework 4.8 |
| Languages | German complete; English complete except MediaPortal/plugin-owned dialogs |
| Internal scaling | Shell 100–175%; native pages use their original layout in the default mode |
| OS DPI | 100% validated; 150/200% still need a fresh-session check |
| Plugins | Native pages Tier A; unknown plugins Tier B; Tier C blocked |

## Trade-off

Original native layouts are smaller than the previous zoomed pages. That is intentional: it preserves readable control colors and prevents fixed-coordinate collisions. Users who prefer the Studio-themed native pages can switch the mode back and restart.

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

- Native pages use MediaPortal's original fonts and coordinates in the default mode; they may look smaller than the Studio shell.
- Individual fixed native controls can still clip; the classic interface remains available.
- MediaPortal and third-party plugin dialogs use their own language and layout.
- Full per-monitor DPI, OS DPI 150/200% fresh-session checks, and clean-VM checks remain pending.
- NUnit migration is deferred.
- Unsigned until SignPath approval.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
