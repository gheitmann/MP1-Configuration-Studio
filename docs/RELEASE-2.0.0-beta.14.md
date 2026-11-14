# MP1 Configuration Studio 2.0.0-beta.14

**Status:** Fourteenth preview. Fixes two regressions and closes major gaps on the Codecs and Plugins pages. Unsigned preview.

## Fixes

- **1.9 interface start fixed.** The sidebar hand-off quoted a runtime path with a trailing backslash, which produced an embedded quote and the error “Illegales Zeichen im Pfad”. The restart now uses the proper Windows argument quoting. Verified: 1.9 shell attaches again.
- **Sidebar hover fixed.** Items no longer stay highlighted after the mouse leaves. Hover is recomputed against the actual pointer position, cleared on navigation, filtering, and wheel scrolling, and only one item can be selected.

## Codecs

The page now mirrors the original tab structure with all core codec groups:

- **Video:** MPEG-2, H.264, HEVC, VC-1, VC-1i, DivX/Xvid.
- **Audio:** MPEG/AC3, AAC, audio renderer, source filter, splitter.
- **TV:** TV video/H.264/HEVC, TV audio/AAC/DD+, TV renderer.
- **Blu-ray:** MPEG-2, H.264, HEVC, VC-1, MPEG/AC3, renderer.
- **DVD:** video, audio, AC3, navigator, renderer, DXVA switch.
- **Video Renderer:** EVR, madVR, madVR for TV/3D, low latency, DRC, 1080p restore.

Codec fields are now **dropdowns populated from the installed DirectShow filters** (video/audio compressors and audio renderers). Encoders and compressors are filtered out so decoder lists stay readable. The current value is always kept even if its filter is no longer registered.

## Plugins

The page now exposes the three switches from the classic plugin context menu:

- **Aktiviert** (`plugins`)
- **Im Home-Menü** (`home`)
- **In My Plugins** (`myplugins`)

The old “Prozess-Plugins” label was wrong and has been removed. Plugin DLLs remain separately switchable. Configuration windows still open through **Klassisch / Plugins**.

## Validation

- Fixture checks: **220/220**.
- 1.9 start re-tested against the production runtime: attach completed, shell visible.
- Hover path tested across several sidebar items; only the selected item remains highlighted afterwards.
- Codec page inspected at 125%; DirectShow dropdown lists contained LAV Video/Audio Decoder entries.

## Still open

- Filter property pages and per-plugin configuration buttons are still classic.
- Additional original subpages (keyboard, screensaver, OSD, views, post-processing) are not yet split into tabs.

## Install

1. Back up the MediaPortal configuration.
2. Close MediaPortal and the original configurator.
3. Download the portable ZIP or setup and verify the SHA256 sidecar.
4. Run the setup (administrator approval required) or extract the portable ZIP completely.
5. Start `MP1ConfigurationStudio.exe` — the native interface opens by default.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
