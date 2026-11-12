# Support

This is a community project for MediaPortal 1.39. Support is best effort, not a commercial service.

## Supported configuration

| Item | Supported |
| --- | --- |
| MediaPortal | 1.39, x64 (x86 only if shipped and listed in the release notes) |
| Windows | Windows 10 22H2, Windows 11 23H2 or newer |
| Runtime | .NET Framework 4.8 |
| Languages | German shell; native labels follow MediaPortal |
| UI scaling | The scale steps listed in the release notes |
| Plugins | Tier A fully tested; Tier B opens and saves; Tier C blocked |

Anything outside this matrix may work, but is not supported.

## Before reporting

1. Back up your MediaPortal configuration.
2. Try `--classic` or the classic menu item. If the original configurator works, the issue is in the Studio shell.
3. Create a support bundle from the error dialog or the help menu.
4. Check the release notes and known limitations.

## What to include

- Studio version and architecture.
- MediaPortal version and architecture.
- Windows version and build.
- Display scaling and Studio scale.
- Language and theme.
- Plugin name and version, if the issue involves a plugin page.
- Exact steps, expected behavior, and actual behavior.
- Support bundle and, if useful, a screenshot.

Do not include `MediaPortal.xml`, credentials, or personal media paths in a public issue.

## Out of scope

- MediaPortal versions other than 1.39.
- TV tuner, remote control, codec, or hardware setup.
- Bugs inside third-party plugins.
- Recovering a configuration that was changed without a backup.

## Security issues

Do not open a public issue. Follow [SECURITY.md](SECURITY.md).
