# MP1 Configuration Studio 2.0.0-beta.21

**Status:** Twenty-first preview. Completes the DSP comfort UI and adds keymap window management. Unsigned preview. Not installed locally (no UAC run).

## New in beta.21

### Music DSP comfort UI

- **Gain:** enable checkbox, -16.0 to +16.0 dB slider, live value, stored as `BASS EffectName="Gain"` with `Gain_dbV`.
- **Dynamic amplification:** enable checkbox and **Soft / Medium / Hard** preset, stored as `DynAmp` with `Preset`.
- **Compressor:** enable checkbox, -25.0 to 0.0 dB threshold slider, stored as `Compressor` with `Threshold` in the same integer format as MediaPortal.
- Unknown BASS effects remain available as generic parameter cards.
- Existing extra parameters in known effects are preserved; disabling removes only the selected effect.
- **VST discovery:** scans `musicplayer\plugins\dsp` recursively, lists DLLs, and adds the selected file as a `<VST PluginDll="…" />` entry.
- **Winamp discovery:** same scan for Winamp DSP DLLs.
- **Test playback:** a BASS-based test dialog with file picker, Play/Stop, and gain volume. The dialog reports BASS errors; DynAmp and Compressor are applied by MediaPortal, not in this preview.

### Keyboard window management

- **Fenster hinzufügen:** creates a new `<window>` context with window ID and description.
- **Fenster löschen:** removes the selected window context and its mappings after confirmation; Global is protected.
- Context list rebuilds after add/delete while unsaved action edits stay in memory.
- Existing quick search, key capture, sound, and duplicate warning remain.

## Validation

- Fixture checks: **224/224** on x64.
- DSP:
  - Gain enabled and saved: `<BASS EffectName="Gain"><Parameter Name="Gain_dbV" Value="0.0" /></BASS>`.
  - VST discovery found a test DLL and saved `<VST PluginDll="TestVST.dll" />`.
  - BASS test playback initialized BASS, opened a WAV stream, and reported **Wiedergabe mit Gain …**.
- Keymap: added window `9999 / Test Window` and verified it in `keymap.xml`; window delete code is implemented and guarded.
- 175% layout for DSP and plugin configuration remains from beta.19/beta.20.
- The installed beta.14 remains active because installing still requires elevation.

## Install

The app is **not installed** by this build; the previously installed version remains active until an elevated copy is approved. The portable ZIP/setup is available from the release.

## License

GPL-3.0-or-later. MediaPortal is a separate project; this package does not redistribute its runtime, plugins, skins, or user configuration. The application icon is the original MediaPortal icon from MediaPortal 1.39.
