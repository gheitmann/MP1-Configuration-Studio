# MP1 Configuration Studio 2.0.0-beta.34

**Status:** Thirty-fourth preview. Improves the MediaPortal plugin skin across skins. Unsigned preview.

## Fixed in beta.34

### Titan and other skins

The plugin skin used `font32` and `font16`, which exist in Backdrop but not in Titan. Titan therefore fell back to a very small default font. The skin now uses fonts that exist in both skins:

- title: `font22`
- rows, status, and footer buttons: `font14`

Focused rows use white text; unfocused rows use a clearly darker gray. This makes the current selection visible even in skins that do not provide the Backdrop list-focus textures.

### Verified skins

- **Backdrop**: full-width focus bar, navigation, value changes, save, back, and home return.
- **Titan**: readable text, visible focus change from **Allgemein** to **System und Laufwerke** with Up/Down, footer controls readable.

## Combined installer verified on the real installation

The beta.33 setup was installed elevated with the plugin component:

- app `2.0.0-beta.33`
- plugin `2.0.0-beta.33+1cb2ce8…`
- skin file present in **10** skins
- plugin entry stays `yes`
- `MediaPortal.xml` retained all restored settings

## Plugin behaviour carried over

- own focus model for Up/Down/Left/Right/OK/Back
- labels refresh immediately after a value change
- direct atomic `MediaPortal.xml` writes with ZIP backup
- no shared `MPSettings` cache, so unrelated settings are not rewritten
- changes require a MediaPortal restart

## Checks

- Fixture checks: **238/238** on x64.
- Plugin build: 0 warnings, 0 errors; `ISetupForm` smoke test passes (window 8917).
- Live test: navigation, toggle, save, backup, back to home, persistence after MediaPortal exit.
- Test configuration restored exactly to its pre-test state.

## Install

The combined setup installs the plugin DLL and skin as optional components. The standalone plugin ZIP remains available for manual install/uninstall.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
