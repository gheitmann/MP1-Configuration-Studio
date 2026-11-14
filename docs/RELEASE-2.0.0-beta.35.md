# MP1 Configuration Studio 2.0.0-beta.35

**Status:** Thirty-fifth preview. Replaces free-text entry with real selection dialogs in the MediaPortal plugin. Unsigned preview.

## New in beta.35

### Selection instead of typing

Fields with a known or discoverable set of values now open a MediaPortal selection dialog instead of the virtual keyboard:

- **Skin** — all installed skins from the MediaPortal `skin` folder, with the current skin first
- **Language** — all installed `language\strings_*.xml` languages with their codes, e.g. `English (en)`, `German (de)`
- **Subtitle font** — all installed font families
- **Blu-ray region** — A, B, C
- **Audio/subtitle language codes** — de, en, fr, it, es, nl, pl, pt, ru, sv, da, fi, no, cs, hu, tr, ja, zh
- **All schema combo fields** — their defined options

Rows that open a selection dialog are marked with `▸`, for example:

```text
Allgemein · Skin: Backdrop ▸
Allgemein · Sprache: English ▸
```

Left/Right cycles through the same choice list without opening the dialog. OK/Enter opens the full list. The current value is placed first so it is preselected.

### Live verification

- **Skin dialog**: listed `Backdrop`, `Apollo`, `DefaultWideHD`, `DefaultWideHD - Copy`, `MayaHD`, `PureVisionHD`, `SkinStudio-*`, `Titan` across 2 pages.
  - `End` + `Enter` selected **Titan**; row updated to `Skin: Titan ▸` and `ungespeichert`.
  - Saved through the plugin; `skin/name=Titan` in `MediaPortal.xml`; ZIP backup created.
  - Skin restored to **Backdrop** after the test.
- **Language dialog**: listed English first, then Basque, Bulgarian, Catalan, Chinese, Croatian, … across 7 pages; cancelled without changing the value.
- Save returned to the home screen; MediaPortal stayed running.

## Checks

- Fixture checks: **238/238** on x64.
- Plugin build: 0 warnings, 0 errors; `ISetupForm` smoke test passes (window 8917).

## Install

The combined setup installs the plugin and skin. The standalone plugin ZIP remains available for manual install/uninstall.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
