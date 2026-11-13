# MP1 Configuration Studio 1.2.0

**Status:** Feature release for MediaPortal 1.39 x64. Unsigned until the planned repository move and SignPath Foundation approval; SHA256 sidecars are published.

## New in 1.2

- **Language selection:** Werkzeuge → Sprache / Tools → Language switches between Deutsch and English. A restart is required.
- **English main shell preview:** navigation groups, toolbar, page header, help/status text, search, and tools menus are translated. The German UI remains the default and complete option.
- **Native page labels** in the English shell use the installed MediaPortal language.
- Includes all 1.1 features: onboarding, visible compatibility tier, native page size, keyboard help, tab order, preflight, single instance, actionable errors, backup manager, High-DPI default, High Contrast fallback.

## Validation

- Fixture checks: 188/188 on x64.
- Native smoke: 27/27 on isolated MediaPortal 1.39 x64.
- Full 175% review: 61/61 pages, 0 value failures, 0 overflow candidates, 5 plugin dialogs.
- Attach time: about 388 ms.
- Original `Configuration.exe` and production `MediaPortal.xml` hashes unchanged after isolated runs.

## Supported matrix

| Item | Support |
| --- | --- |
| MediaPortal | 1.39 x64 |
| Windows | Windows 11 23H2 or newer |
| Runtime | .NET Framework 4.8 |
| Languages | German complete; English main-shell preview |
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

- English localization is a main-shell preview. Onboarding, keyboard help, error, backup, and support-bundle dialogs still contain German text; full dialog localization is planned for 1.3.
- Unsigned build until SignPath approval.
- High-DPI behavior is helper-tested; a real 150/200% session remains pending.
- Clean Windows 11 VM install and elevated setup uninstall remain pending.
- Per-monitor DPI and NUnit migration are planned for 1.3.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration.
