# Validation — v0.1.0-alpha.1

Application build: **1.39.0.14**. Tests ran against MediaPortal **1.39 x64**, using an isolated copy of its runtime and configuration data.

| Check | Result |
| --- | --- |
| x64 UI/Win32 fixture checks | 95 / 95 |
| x86 UI/Win32 fixture checks | 95 / 95 |
| Native core integration checks | 27 / 27 |
| Original sections activated | 61 / 61 |
| Sections preserving values across theme/size changes | 61 / 61 |
| Writable sections saving successfully | 57 / 57 |
| Actual field changes saved, loaded and restored | 68 / 68 |
| Native detail tabs inspected | 56 |
| Original plugin dialogs checked | 5 |
| Original global OK save, fresh-process reload and restoration | Passed |

Theme/size checks covered light/dark and 100%, 125%, 175%. The final automated scan found no geometry overflow candidates or low-contrast list cells in the measured visible content; this is not a claim of exhaustive visual coverage.

Four sections have no own file persistence. MovieViews and MusicViews were verified in fresh processes because their original LoadSettings implementation is not reentrant. Five plugin forms checked: Home, Topbar, RSS Feed, ViewModeSwitcher and MusicDBReorg. These checks exercised opening, theming, value preservation and Cancel without changing their configuration files; they do not certify every plugin feature.

The native Configuration.exe and main MediaPortal.xml hashes were monitored unchanged during the final isolated runs. Test mode also redirects the native debug path into the isolated runtime. Local test receipts and user configuration are deliberately excluded from the public distribution.

Still open: native x86 runtime, hardware/TV/remote combinations, all third-party plugin workflows, broader DPI/multi-monitor testing and independent user acceptance. The alpha is suitable for testing, not a stable-release certification.
