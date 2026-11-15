# MP1 Configuration Studio 2.0.0-beta.41

**Status:** Forty-first preview. Completes the GitHub Actions release workflow; application behaviour is unchanged since beta.39. Unsigned preview.

## CI fixes

- **Build:** `LangVersion` is pinned to 12.0 in the application, diagnostics, and plugin projects. The runner's newer compiler had interpreted `field` as the contextual keyword inside a property accessor and failed on `field.Text`. The member is also renamed to `fieldChoice`, and the helper `Text(...)` became `TextInput(...)`.
- **Setup:** the workflow now builds the MediaPortal plugin before Inno Setup runs. The setup script includes `plugin\bin\Release\MP1.ConfigurationStudio.Plugin.dll`, which did not exist on a clean runner.
- **Assets:** the workflow now also packages the standalone plugin ZIP (DLL, skin, installer script, readme, release notes) so CI releases match the manual release layout.

## Validation

- Fixture checks: **252/252** on x64.
- Local application and plugin builds: 0 errors with the pinned language version.
- The CI pipeline now performs: build + tests, plugin build, portable ZIP, plugin ZIP, setup creation, checksums, draft release.

## Install

No functional reinstall is required; the build matches beta.39 apart from version metadata. The combined setup still installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
