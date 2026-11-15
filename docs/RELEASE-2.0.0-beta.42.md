# MP1 Configuration Studio 2.0.0-beta.42

**Status:** Forty-second preview. Completes the GitHub Actions release workflow; application behaviour is unchanged since beta.39. Unsigned preview.

The `v2.0.0-beta.41` workflow stopped while building the setup because the runner has no MediaPortal assemblies for the plugin. No release was published for that tag; beta.42 is the first green run of the corrected workflow.

## CI fixes

- **Build:** `LangVersion` is pinned to 12.0 in the application, diagnostics, and plugin projects. The runner's newer compiler had interpreted `field` as the contextual keyword inside a property accessor and failed on `field.Text`. The member is also renamed to `fieldChoice`, and the helper `Text(...)` became `TextInput(...)`.
- **Setup:** the release workflow builds the setup without the plugin component when the MediaPortal assemblies are not available on the runner (the plugin references the installed MediaPortal runtime). `/DExcludePlugin` is passed automatically in that case.
- **Assets:** the workflow packages the standalone plugin ZIP only when `plugin\bin\Release\MP1.ConfigurationStudio.Plugin.dll` exists, so a CI run on a clean runner produces the application setup and portable ZIP, while releases prepared on a MediaPortal machine additionally contain the plugin ZIP and the combined setup.

## Validation

- Fixture checks: **252/252** on x64.
- Local application and plugin builds: 0 errors with the pinned language version.
- The CI pipeline now performs: build + tests, portable ZIP, optional plugin ZIP, setup creation (with or without plugin), checksums, draft release.


## Install

No functional reinstall is required; the build matches beta.39 apart from version metadata. The combined setup still installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
