# MP1 Configuration Studio 2.0.0-beta.40

**Status:** Fortieth preview. Build and release-workflow fix; no functional changes to the configuration UI. Unsigned preview.

## Why the GitHub Actions release failed

The runner built with a newer .NET SDK than the local build because `LangVersion` was set to `latest`. The newer C# compiler treats `field` as a contextual keyword inside property accessors, so `NativeViewsEditor.FilterDialog` failed with:

```
NativeViewsEditor.cs(841,54): error CS1061: 'string' does not contain a definition for 'Text'
```

The `Field` property returned `field.Text`, and the new compiler resolved `field` to that property's synthesized backing field (a string) instead of the `ComboBox` member.

## Fix

- `LangVersion` is pinned to **12.0** in the application, diagnostics, and plugin projects, so CI and local builds use the same language rules regardless of the installed SDK.
- The ambiguous `ComboBox field` member is renamed to `fieldChoice`.
- The helper `Text(...)` is renamed to `TextInput(...)`, removing the `CS0108` warning about hiding `UserControl.Text`.

## Validation

- Fixture checks: **252/252** on x64.
- Application and plugin build with 0 errors under the pinned language version.
- No change to pages, values, persistence, glass removal, tab spacing, or the MediaPortal plugin.

## Install

No reinstall is required for functional reasons; the build is identical to beta.39. The combined setup still installs application and optional plugin together.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
